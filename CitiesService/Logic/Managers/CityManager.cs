using AutoMapper;
using CitiesService.Data;
using CitiesService.Data.DatabaseModels;
using CitiesService.Dto;
using CitiesService.Logic.Helpers;
using CitiesService.Logic.Managers.Contracts;
using CitiesService.Logic.Repositories.Contracts;
using CitiesService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CitiesService.Logic.Managers
{
    public class CityManager : ICityManager
    {
        private readonly FileUrlsAndPaths _fileUrlsAndPaths;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<CityInfo> _cityInfoRepo;
        private readonly ILogger<CityManager> _logger;

        public CityManager(
            IOptions<FileUrlsAndPaths> options,
            IMapper mapper,
            IGenericRepository<CityInfo> cityInfoRepo,
            ILogger<CityManager> logger)
        {
            _fileUrlsAndPaths = options.Value;
            _mapper = mapper;
            _cityInfoRepo = cityInfoRepo;
            _logger = logger;
        }

        public async Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10)
        {
            List<CityDto> cities = new();

            if (!string.IsNullOrEmpty(cityName) && limit > 0)
            {
                var cityInfos = await _cityInfoRepo.FindAll(
                    c => c.Name.Contains(cityName),
                    takeNumberOfRows: limit);

                if (cityInfos != null && cityInfos.Any())
                {
                    var cityInfoList = cityInfos.ToList();
                    cityInfoList = cityInfoList.GroupBy(x => x.Name).Select(x => x.First()).ToList();

                    cities = _mapper.Map<List<CityDto>>(cityInfoList);

                    _logger.LogInformation($"Properly got cities by name: {cityName}");
                }
                else
                {
                    _logger.LogWarning($"Didn't find any city for given name: {cityName}");
                }
            }

            return cities;
        }

        public async Task<CitiesPaginationDto> GetCitiesPagination(int numberOfCities = 25, int pageNumber = 1)
        {
            CitiesPaginationDto citiesPaginationDto = new();

            if (pageNumber >= 1 && numberOfCities > 1)
            {
                var howManyToSkip = pageNumber > 1 ? numberOfCities * (pageNumber - 1) : 0;

                var totalNumberOfCities = (await _cityInfoRepo.FindAll()).Count();
                var cityInfos = await _cityInfoRepo.FindAll(takeNumberOfRows: numberOfCities, skipNumberOfRows: howManyToSkip);
                var cityDtoList = _mapper.Map<List<CityDto>>(cityInfos.ToList());

                citiesPaginationDto = new() 
                { 
                    Cities = cityDtoList,
                    NumberOfAllCities = totalNumberOfCities
                };
            }

            return citiesPaginationDto;
        }

        public async Task<bool> DownloadCityFile()
        {
            if (!File.Exists(_fileUrlsAndPaths.CompressedCityListFilePath))
            {
                using var client = new WebClient();
                client.DownloadFile(_fileUrlsAndPaths.CityListFileUrl, _fileUrlsAndPaths.CompressedCityListFilePath);
            }

            if (!File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath))
            {
                var fileInfo = new FileInfo(_fileUrlsAndPaths.DecompressedCityListFilePath);

                GzipHelper.Decompress(fileInfo);
            }

            return File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath);
        }

        public async Task<bool> SaveCitiesFromFileToDatabase()
        {
            var result = false;

            var anyCityExists = await _cityInfoRepo.CheckIfExists(c => c.Id != default);

            if (!anyCityExists)
            {
                await DownloadCityFile();

                if (File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath))
                {
                    List<CityDto> citiesFromJson = new();

                    using StreamReader streamReader = new(_fileUrlsAndPaths.DecompressedCityListFilePath);
                    string json = streamReader.ReadToEnd();
                    citiesFromJson = JsonConvert.DeserializeObject<List<CityDto>>(json);

                    var cityInfos = _mapper.Map<List<CityInfo>>(citiesFromJson);

                    await _cityInfoRepo.CreateRange(cityInfos);
                    result = await _cityInfoRepo.Save();
                }
            }

            return result;
        }
    }
}
