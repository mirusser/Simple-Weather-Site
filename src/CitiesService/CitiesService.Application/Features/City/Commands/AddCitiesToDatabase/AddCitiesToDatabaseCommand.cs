using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.ApplicationCommon.Helpers;
using CitiesService.ApplicationCommon.Interfaces.Persistance;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;

namespace CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;

public class AddCitiesToDatabaseCommand : IRequest<ErrorOr<AddCitiesToDatabaseResult>>;

public class AddCitiesToDatabaseHandler(
    IGenericRepository<CityInfo> cityInfoRepo,
    IOptions<FileUrlsAndPaths> options,
    IMapper mapper,
    HttpClient httpClient) : IRequestHandler<AddCitiesToDatabaseCommand, ErrorOr<AddCitiesToDatabaseResult>>
{
    private readonly IGenericRepository<CityInfo> cityInfoRepo = cityInfoRepo;
    private readonly FileUrlsAndPaths fileUrlsAndPaths = options.Value;
    private readonly IMapper mapper = mapper;
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<ErrorOr<AddCitiesToDatabaseResult>> Handle(
        AddCitiesToDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var isSuccess = await SaveCitiesFromFileToDatabase();

        return new AddCitiesToDatabaseResult { IsSuccess = isSuccess };
    }

    // TODO: add logging
    private async Task<bool> SaveCitiesFromFileToDatabase()
    {
        var anyCityExists = await cityInfoRepo.CheckIfExists(c => c.Id != default);
        if (anyCityExists)
        {
            return false;
        }

        var downloadResult = await DownloadCityFileAsync();
        if (!downloadResult)
        {
            return false;
        }

        using StreamReader streamReader = new(fileUrlsAndPaths.DecompressedCityListFilePath);
        string? json = streamReader.ReadToEnd();

        // TODO: create accessible globally JsonSerializerOptions (and cache it) (to think about it)
        List<GetCityResult>? citiesFromJson = JsonSerializer.Deserialize<List<GetCityResult>>(
            json,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        citiesFromJson ??= [];

        var cityInfos = mapper.Map<List<CityInfo>>(citiesFromJson);

        await cityInfoRepo.CreateRange(cityInfos);
        return await cityInfoRepo.Save();
    }

    public async Task<bool> DownloadCityFileAsync()
    {
        if (!File.Exists(fileUrlsAndPaths.CompressedCityListFilePath))
        {
            await DownloadFileAsync(fileUrlsAndPaths.CityListFileUrl, fileUrlsAndPaths.CompressedCityListFilePath);
        }

        if (!File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath))
        {
            var fileInfo = new FileInfo(fileUrlsAndPaths.CompressedCityListFilePath);
            GzipHelper.Decompress(fileInfo);
        }

        return File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath);
    }

    private async Task DownloadFileAsync(string requestUri, string filename)
    {
        using var response = await _httpClient
            .GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream);
    }
}