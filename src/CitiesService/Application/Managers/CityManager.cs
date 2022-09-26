﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CitiesService.Application.Dto;
using CitiesService.Application.Helpers;
using CitiesService.Application.Interfaces.Managers;
using CitiesService.Application.Interfaces.Repositories;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CitiesService.Application.Managers;

public class CityManager : ICityManager
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<CityInfo> _cityInfoRepo;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;

    private readonly FileUrlsAndPaths _fileUrlsAndPaths;
    private readonly MemoryCacheEntryOptions _cacheExpiryOptions;
    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

    public CityManager(
        IOptions<FileUrlsAndPaths> options,
        IMapper mapper,
        IGenericRepository<CityInfo> cityInfoRepo,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache)
    {
        _fileUrlsAndPaths = options.Value;
        _mapper = mapper;
        _cityInfoRepo = cityInfoRepo;
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
    public async Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10)
    {
        var cities = new List<CityDto>();

        var cacheKey = $"GetCitiesByName-{nameof(cityName)}-{cityName}-{nameof(limit)}-{limit}";

        if (_memoryCache.TryGetValue(cacheKey, out cities) && cities is not null)
            return cities;

        if (limit <= 0)
            return new List<CityDto>();

        var cityInfos = _cityInfoRepo.FindAll(
            c => c.Name.Contains(cityName),
            orderByExpression: x => x.OrderBy(c => c.Id),
            takeNumberOfRows: limit);

        if (cityInfos != null && cityInfos.Any())
        {
            var cityInfoList = await cityInfos.ToListAsync();
            cityInfoList = cityInfoList.GroupBy(x => x.Name).Select(x => x.First()).ToList();

            cities = _mapper.Map<List<CityDto>>(cityInfoList);

            _memoryCache.Set(cacheKey, cities, _cacheExpiryOptions);
        }

        return cities ?? new();
    }

    public async Task<CitiesPaginationDto> GetCitiesPaginationDto(int numberOfCities = 25, int pageNumber = 1)
    {
        var cityInfoPaginationDto = await GetCitiesInfoPagination(numberOfCities, pageNumber);
        var cities = cityInfoPaginationDto.CityInfos != null ? _mapper.Map<List<CityDto>>(cityInfoPaginationDto.CityInfos) : new List<CityDto>();

        CitiesPaginationDto citiesPaginationDto = new()
        {
            Cities = cities,
            NumberOfAllCities = cityInfoPaginationDto.NumberOfAllCities
        };

        return citiesPaginationDto;
    }

    //uses redis caching
    public async Task<CityInfoPaginationDto> GetCitiesInfoPagination(int numberOfCities = 25, int pageNumber = 1)
    {
        var cacheKey = $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";
        string serializedCitiesInfoPagination;
        var redisCitiesPaginationDto = await _distributedCache.GetAsync(cacheKey);

        CityInfoPaginationDto result = new();

        if (redisCitiesPaginationDto != null)
        {
            serializedCitiesInfoPagination = Encoding.UTF8.GetString(redisCitiesPaginationDto);
            result = JsonSerializer.Deserialize<CityInfoPaginationDto>(serializedCitiesInfoPagination, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            result ??= new();
        }
        else
        {
            result.NumberOfAllCities =
                _cityInfoRepo
                .FindAll(orderByExpression: x => x.OrderBy(c => c.Name))
                .Count();

            if (pageNumber >= 1 && numberOfCities >= 1)
            {
                var howManyToSkip = pageNumber > 1 ? numberOfCities * (pageNumber - 1) : 0;

                result.CityInfos =
                    _cityInfoRepo
                    .FindAll(orderByExpression: x => x.OrderBy(c => c.Name), takeNumberOfRows: numberOfCities, skipNumberOfRows: howManyToSkip)
                    .ToList();
            }

            serializedCitiesInfoPagination =
                JsonSerializer
                .Serialize(
                    result,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            redisCitiesPaginationDto = Encoding.UTF8.GetBytes(serializedCitiesInfoPagination);
            await _distributedCache.SetAsync(cacheKey, redisCitiesPaginationDto, _distributedCacheEntryOptions);
        }

        return result;
    }

    public async Task<int> GetCountOfAllCities()
    {
        return await _cityInfoRepo.FindAll().CountAsync();
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
                using StreamReader streamReader = new(_fileUrlsAndPaths.DecompressedCityListFilePath);
                string json = streamReader.ReadToEnd();
                List<CityDto> citiesFromJson = JsonSerializer.Deserialize<List<CityDto>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                citiesFromJson ??= new();

                var cityInfos = _mapper.Map<List<CityInfo>>(citiesFromJson);

                await _cityInfoRepo.CreateRange(cityInfos);
                result = await _cityInfoRepo.Save();
            }
        }

        return result;
    }
}