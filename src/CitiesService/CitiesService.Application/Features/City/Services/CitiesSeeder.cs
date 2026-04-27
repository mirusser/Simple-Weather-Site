using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Helpers;
using CitiesService.Application.Common.Exceptions;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Telemetry;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using Common.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace CitiesService.Application.Features.City.Services;

public sealed class CitiesSeeder(
    IGenericRepository<CityInfo> cityInfoRepo,
    ISeedLockProvider seedLockProvider,
    IOptions<FileUrlsAndPaths> fileUrlsAndPathsOptions,
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<CitiesSeeder> logger,
    JsonSerializerOptions jsonSerializerOptions) : ICitiesSeeder
{
    private readonly FileUrlsAndPaths fileUrlsAndPaths = fileUrlsAndPathsOptions.Value;

    public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken)
    {
        const string operation = "CitiesSeedIfEmpty";
        using var activity = CitiesTelemetry.StartApplicationActivity(operation);
        var startedAt = Stopwatch.GetTimestamp();
        var result = true;

        try
        {
            // Quick pre-check (fast path)
            if (await cityInfoRepo.CheckIfExistsAsync(c => c.Id != 0, cancellationToken))
            {
                logger.LogInformation("Cities already exist. Skipping seeding.");
                RecordSeedingResult("already_exists");
                return result;
            }

            // Acquire a cross-instance lock (only one replica seeds)
            await using var seedLock = await seedLockProvider.TryAcquireAsync("CitiesSeed", cancellationToken);
            if (seedLock is null)
            {
                logger.LogInformation("Another instance is seeding cities. Skipping seeding.");
                RecordSeedingResult("lock_not_acquired");
                return result;
            }

            // Re-check under lock (important!)
            if (await cityInfoRepo.CheckIfExistsAsync(c => c.Id != 0, cancellationToken))
            {
                logger.LogInformation("Cities already exist (after lock). Skipping seeding.");
                RecordSeedingResult("already_exists");
                return result;
            }

            logger.LogInformation("Seeding cities...");

            result = await SaveCitiesFromFileToDatabase(cancellationToken);
            RecordSeedingResult(result ? "seeded" : "failed");

            logger.LogInformation("Seeding cities finished. Success={Success}", result);

            return result;
        }
        catch (Exception ex)
        {
            var errorType = ex.GetType().Name;
            activity?.SetStatus(ActivityStatusCode.Error, errorType);
            activity?.SetTag("result", "exception");
            activity?.SetTag("error_type", errorType);
            CitiesTelemetry.RecordSeedingRun(
                operation,
                Stopwatch.GetElapsedTime(startedAt),
                "exception",
                errorType);

            throw;
        }

        void RecordSeedingResult(string outcome)
        {
            activity?.SetStatus(outcome is "seeded" or "already_exists" or "lock_not_acquired"
                ? ActivityStatusCode.Ok
                : ActivityStatusCode.Error);
            activity?.SetTag("result", outcome);
            CitiesTelemetry.RecordSeedingRun(operation, Stopwatch.GetElapsedTime(startedAt), outcome);
        }
    }

    private async Task<bool> SaveCitiesFromFileToDatabase(CancellationToken cancellationToken)
    {
        using var activity = CitiesTelemetry.StartApplicationActivity("CitiesSeedDatabaseWrite");
        var downloadResult = await SaveCitiesToFileAsync(cancellationToken);
        if (!downloadResult)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("result", "download_failed");
            return false;
        }

        using StreamReader streamReader = new(fileUrlsAndPaths.DecompressedCityListFilePath);
        var json = await streamReader.ReadToEndAsync(cancellationToken);

        var citiesFromJson = JsonSerializer.Deserialize<List<GetCityResult>>(
            json,
            jsonSerializerOptions);
        citiesFromJson ??= [];

        var cityInfos = citiesFromJson
            .Select(c => new CityInfo()
            {
                CityId = c.Id,
                CountryCode = c.Country ?? string.Empty,
                Lat = c.Coord?.Lat ?? 0,
                Lon = c.Coord?.Lon ?? 0,
                Name = c.Name ?? string.Empty,
                State = c.State
            });

        await cityInfoRepo.CreateRangeAsync(cityInfos, cancellationToken);

        try
        {
            var result = await cityInfoRepo.SaveAsync(cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("result", result ? "success" : "failed");
            return result;
        }
        catch (PersistenceUpdateException ex)
        {
            // If you add unique index, a race/manual call can cause duplicates -> treat as "seeded enough"
            logger.LogWarning(ex, "City seeding encountered a DB update exception (possible duplicates).");
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("result", "concurrency_accepted");
            return true;
        }
    }

    private async Task<bool> SaveCitiesToFileAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(fileUrlsAndPaths.CompressedCityListFilePath))
        {
            var directory = Path.GetDirectoryName(fileUrlsAndPaths.CompressedCityListFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await DownloadFileAsync(
                fileUrlsAndPaths.CityListFileUrl,
                fileUrlsAndPaths.CompressedCityListFilePath,
                cancellationToken);
        }

        if (!File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath))
        {
            var directory = Path.GetDirectoryName(fileUrlsAndPaths.DecompressedCityListFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileInfo = new FileInfo(fileUrlsAndPaths.CompressedCityListFilePath);
            await GzipHelper.DecompressAsync(fileInfo, logger, cancellationToken);
        }

        return File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath);
    }

    private async Task DownloadFileAsync(
        string requestUri,
        string filename,
        CancellationToken cancellationToken)
    {
        using var activity = CitiesTelemetry.StartApplicationActivity("CitiesDownloadCityList");
        var client = clientFactory.CreateClient();
        var pipeline = pipelineProvider.GetPipeline(PipelineNames.Default);
        HttpResponseMessage? response = null;

        try
        {
            response = await pipeline.ExecuteAsync(
                async token => await client.GetAsync(requestUri, token),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("result", "failed");
                activity?.SetTag("http.status_code", (int)response.StatusCode);

                logger.LogError(
                    "Failed to download file from {RequestUri}. Status: {StatusCode} ({ReasonPhrase}). Body: {Body}",
                    requestUri,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    body);
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using FileStream fileStream = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("result", "success");
            activity?.SetTag("http.status_code", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            activity?.SetTag("result", "exception");
            activity?.SetTag("error_type", ex.GetType().Name);
            logger.LogError(ex, "Unexpected error downloading file from {RequestUri}", requestUri);
            throw;
        }
        finally
        {
            response?.Dispose();
        }
    }
}
