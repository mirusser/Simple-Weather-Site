using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Helpers;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using Common.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace CitiesService.Application.Features.City.Services;

public sealed class CitiesSeeder(
    IGenericRepository<CityInfo> cityInfoRepo,
    IOptions<FileUrlsAndPaths> fileUrlsAndPathsOptions,
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<CitiesSeeder> logger,
    JsonSerializerOptions jsonSerializerOptions) : ICitiesSeeder
{
    private readonly FileUrlsAndPaths fileUrlsAndPaths = fileUrlsAndPathsOptions.Value;

    public async Task SeedIfEmptyAsync(CancellationToken ct)
    {
        // Quick pre-check (fast path)
        if (await cityInfoRepo.CheckIfExistsAsync(c => c.Id != 0, ct))
        {
            logger.LogInformation("Cities already exist. Skipping seeding.");
            return;
        }

        // Acquire a cross-instance lock (only one replica seeds)
        if (!await cityInfoRepo.TryAcquireSeedLockAsync(ct))
        {
            logger.LogInformation("Another instance is seeding cities. Skipping seeding.");
            return;
        }

        // Re-check under lock (important!)
        if (await cityInfoRepo.CheckIfExistsAsync(c => c.Id != 0, ct))
        {
            logger.LogInformation("Cities already exist (after lock). Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding cities...");

        var ok = await SaveCitiesFromFileToDatabase(ct);

        logger.LogInformation("Seeding cities finished. Success={Success}", ok);
    }

    
    private async Task<bool> SaveCitiesFromFileToDatabase(CancellationToken cancellationToken)
    {
        var downloadResult = await SaveCitiesToFileAsync(cancellationToken);
        if (!downloadResult)
        {
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
            return await cityInfoRepo.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // If you add unique index, a race/manual call can cause duplicates -> treat as "seeded enough"
            logger.LogWarning(ex, "City seeding encountered a DB update exception (possible duplicates).");
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error downloading file from {RequestUri}", requestUri);
            throw;
        }
        finally
        {
            response?.Dispose();
        }
    }
}