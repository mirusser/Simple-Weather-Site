using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Helpers;
using CitiesService.Application.Common.Interfaces.Persistance;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Models.Dto;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;

namespace CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;

public class AddCitiesToDatabaseCommand : IRequest<ErrorOr<AddCitiesToDatabaseResult>>
{
}

public class AddCitiesToDatabaseHandler : IRequestHandler<AddCitiesToDatabaseCommand, ErrorOr<AddCitiesToDatabaseResult>>
{
    private readonly IGenericRepository<CityInfo> cityInfoRepo;
    private readonly FileUrlsAndPaths fileUrlsAndPaths;
    private readonly IMapper mapper;

    public AddCitiesToDatabaseHandler(
        IGenericRepository<CityInfo> cityInfoRepo,
        IOptions<FileUrlsAndPaths> options,
        IMapper mapper)
    {
        fileUrlsAndPaths = options.Value;
        this.cityInfoRepo = cityInfoRepo;
        this.mapper = mapper;
    }

    public async Task<ErrorOr<AddCitiesToDatabaseResult>> Handle(
        AddCitiesToDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var isSuccess = await SaveCitiesFromFileToDatabase();

        return new AddCitiesToDatabaseResult { IsSuccess = isSuccess };
    }

    private async Task<bool> SaveCitiesFromFileToDatabase()
    {
        var result = false;

        var anyCityExists = await cityInfoRepo.CheckIfExists(c => c.Id != default);
        if (!anyCityExists)
        {
            DownloadCityFile();

            if (File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath))
            {
                using StreamReader streamReader = new(fileUrlsAndPaths.DecompressedCityListFilePath);
                string json = streamReader.ReadToEnd();
                List<GetCityResult> citiesFromJson = JsonSerializer.Deserialize<List<GetCityResult>>(
                    json,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                citiesFromJson ??= new();

                var cityInfos = mapper.Map<List<CityInfo>>(citiesFromJson);

                await cityInfoRepo.CreateRange(cityInfos);
                result = await cityInfoRepo.Save();
            }
        }

        return result;
    }

    private bool DownloadCityFile()
    {
        if (!File.Exists(fileUrlsAndPaths.CompressedCityListFilePath))
        {
            using var client = new WebClient(); //TODO: this WebClient is obsolete: update and refactor
            client.DownloadFile(
                fileUrlsAndPaths.CityListFileUrl,
                fileUrlsAndPaths.CompressedCityListFilePath);
        }

        if (!File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath))
        {
            var fileInfo = new FileInfo(fileUrlsAndPaths.DecompressedCityListFilePath);

            GzipHelper.Decompress(fileInfo);
        }

        return File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath);
    }
}