using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Common.Helpers;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using Common.Infrastructure.Settings;
using Common.Mediator;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;

public class AddCitiesToDatabaseCommand : IRequest<ErrorOr<AddCitiesToDatabaseResult>>;

public class AddCitiesToDatabaseHandler(
    IGenericRepository<CityInfo> cityInfoRepo,
    IOptions<FileUrlsAndPaths> fileUrlsAndPathsOptions,
    IOptions<ResiliencePipeline> resiliencePipelineOptions,
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<AddCitiesToDatabaseHandler> logger,
    JsonSerializerOptions jsonSerializerOptions)
    : IRequestHandler<AddCitiesToDatabaseCommand, ErrorOr<AddCitiesToDatabaseResult>>
{
    private readonly FileUrlsAndPaths fileUrlsAndPaths = fileUrlsAndPathsOptions.Value;

    public async Task<ErrorOr<AddCitiesToDatabaseResult>> Handle(
        AddCitiesToDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var anyCityExists = await cityInfoRepo
            .CheckIfExistsAsync(c => c.Id != 0, cancellationToken);

        if (anyCityExists)
        {
            return new AddCitiesToDatabaseResult { IsSuccess = false, IsAlreadyAdded = anyCityExists };
        }

        var isSuccess = await SaveCitiesFromFileToDatabase(cancellationToken);

        return new AddCitiesToDatabaseResult { IsSuccess = isSuccess, IsAlreadyAdded = anyCityExists };
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
        return await cityInfoRepo.SaveAsync(cancellationToken);
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
        var pipeline = pipelineProvider.GetPipeline(resiliencePipelineOptions.Value.Name);
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