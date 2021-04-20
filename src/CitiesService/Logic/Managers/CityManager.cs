using AutoMapper;
using CitiesService.Data;
using CitiesService.Data.DatabaseModels;
using CitiesService.Dto;
using CitiesService.Logic.Helpers;
using CitiesService.Logic.Managers.Contracts;
using CitiesService.Logic.Repositories.Contracts;
using CitiesService.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CitiesService.Logic.Managers
{
    public class CityManager : ICityManager
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<CityInfo> _cityInfoRepo;
        private readonly ILogger<CityManager> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;

        private readonly FileUrlsAndPaths _fileUrlsAndPaths;
        private readonly MemoryCacheEntryOptions _cacheExpiryOptions;
        private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

        public CityManager(
            IOptions<FileUrlsAndPaths> options,
            IMapper mapper,
            IGenericRepository<CityInfo> cityInfoRepo,
            ILogger<CityManager> logger,
            IMemoryCache memoryCache,
            IDistributedCache distributedCache)
        {
            _fileUrlsAndPaths = options.Value;
            _mapper = mapper;
            _cityInfoRepo = cityInfoRepo;
            _logger = logger;
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;

            _cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _distributedCacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));
        }

        //uses in-memory caching
        public List<CityDto> GetCitiesByName(string cityName, int limit = 10)
        {
            var cacheKey = $"GetCitiesByName-{nameof(cityName)}-{cityName}-{nameof(limit)}-{limit}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<CityDto> cities))
            {
                if (cities == null) cities = new();

                if (!string.IsNullOrEmpty(cityName) && limit > 0)
                {
                    var cityInfos = _cityInfoRepo.FindAll(
                        c => c.Name.Contains(cityName),
                        takeNumberOfRows: limit);

                    if (cityInfos != null && cityInfos.Any())
                    {
                        var cityInfoList = cityInfos.ToList();
                        cityInfoList = cityInfoList.GroupBy(x => x.Name).Select(x => x.First()).ToList();

                        cities = _mapper.Map<List<CityDto>>(cityInfoList);

                        _memoryCache.Set(cacheKey, cities, _cacheExpiryOptions);
                    }
                    else
                    {
                        _logger.LogWarning($"{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}: Didn't find any city for given name: {cityName}");
                    }
                }
            }

            return cities;
        }

        //uses redis caching
        public async Task<CitiesPaginationDto> GetCitiesPagination(int numberOfCities = 25, int pageNumber = 1)
        {
            var cacheKey = $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";
            string serializedCitiesPaginationDto;
            var redisCitiesPaginationDto = await _distributedCache.GetAsync(cacheKey);

            CitiesPaginationDto citiesPaginationDto = new();

            if (redisCitiesPaginationDto != null)
            {
                serializedCitiesPaginationDto = Encoding.UTF8.GetString(redisCitiesPaginationDto);
                citiesPaginationDto = JsonSerializer.Deserialize<CitiesPaginationDto>(serializedCitiesPaginationDto);
            }
            else
            {
                citiesPaginationDto.NumberOfAllCities = _cityInfoRepo.FindAll().Count();

                if (pageNumber >= 1 && numberOfCities >= 1)
                {
                    var howManyToSkip = pageNumber > 1 ? numberOfCities * (pageNumber - 1) : 0;

                    var cityInfos = _cityInfoRepo.FindAll(takeNumberOfRows: numberOfCities, skipNumberOfRows: howManyToSkip);
                    var cityDtoList = _mapper.Map<List<CityDto>>(cityInfos.ToList());

                    citiesPaginationDto.Cities = cityDtoList;
                }

                serializedCitiesPaginationDto = JsonSerializer.Serialize(citiesPaginationDto);
                redisCitiesPaginationDto = Encoding.UTF8.GetBytes(serializedCitiesPaginationDto);
                await _distributedCache.SetAsync(cacheKey, redisCitiesPaginationDto, _distributedCacheEntryOptions);
            }

            return citiesPaginationDto;
        }

        public bool DownloadCityFile()
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
                DownloadCityFile();

                if (File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath))
                {
                    List<CityDto> citiesFromJson = new();

                    using StreamReader streamReader = new(_fileUrlsAndPaths.DecompressedCityListFilePath);
                    string json = streamReader.ReadToEnd();
                    citiesFromJson = JsonSerializer.Deserialize<List<CityDto>>(json);

                    var cityInfos = _mapper.Map<List<CityInfo>>(citiesFromJson);

                    await _cityInfoRepo.CreateRange(cityInfos);
                    result = await _cityInfoRepo.Save();
                }
            }

            return result;
        }
    }
}
