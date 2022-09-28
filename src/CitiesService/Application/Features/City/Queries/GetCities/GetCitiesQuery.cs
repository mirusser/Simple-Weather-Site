using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistance;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Models.Dto;
using CitiesService.Domain.Common.Errors;
using CitiesService.Domain.Entities;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CitiesService.Application.Features.City.Queries.GetCities;

public class GetCitiesQuery : IRequest<ErrorOr<GetCitiesResult>>
{
    public string CityName { get; set; } = null!;
    public int Limit { get; set; } = 10;
}

public class GetCitiesHandler
    : IRequestHandler<GetCitiesQuery, ErrorOr<GetCitiesResult>>
{
    private readonly IGenericRepository<CityInfo> cityInfoRepo;
    private readonly IMapper mapper;
    private readonly IMemoryCache memoryCache;

    private readonly MemoryCacheEntryOptions cacheExpiryOptions; //TODO: add options to settings (?)

    public GetCitiesHandler(
        IGenericRepository<CityInfo> cityInfoRepo,
        IMapper mapper,
        IMemoryCache memoryCache)
    {
        this.cityInfoRepo = cityInfoRepo;
        this.mapper = mapper;
        this.memoryCache = memoryCache;

        cacheExpiryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddMinutes(5),
            Priority = CacheItemPriority.High,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
    }

    public async Task<ErrorOr<GetCitiesResult>> Handle(
        GetCitiesQuery request,
        CancellationToken cancellationToken)
    {
        //TODO: move this to validator
        if (string.IsNullOrEmpty(request.CityName) ||
            string.IsNullOrEmpty(request.CityName.TrimStart()) ||
            string.IsNullOrEmpty(request.CityName.TrimEnd()))
        {
            return new(); //TODO
        }

        request.CityName = request.CityName.TrimStart().TrimEnd();

        var cities = await GetCitiesByName(request.CityName, request.Limit);

        if (cities is null)
        {
            return Errors.City.CityNotFound;
        }

        return new GetCitiesResult { Cities = cities };
    }

    private async Task<List<GetCityResult>?> GetCitiesByName(string cityName, int limit = 10)
    {
        var cities = new List<GetCityResult>();

        var cacheKey = $"GetCitiesByName-{nameof(cityName)}-{cityName}-{nameof(limit)}-{limit}";

        if (memoryCache.TryGetValue(cacheKey, out cities) && cities is not null)
        {
            return cities;
        }

        if (limit <= 0)
        {
            return new List<GetCityResult>();
        }

        var cityInfos = cityInfoRepo.FindAll(
            c => c.Name.Contains(cityName),
            orderByExpression: x => x.OrderBy(c => c.Id),
            takeNumberOfRows: limit);

        if (cityInfos?.Any() == true)
        {
            var cityInfoList = await cityInfos.ToListAsync();
            cityInfoList = cityInfoList.GroupBy(x => x.Name).Select(x => x.First()).ToList();

            cities = mapper.Map<List<GetCityResult>>(cityInfoList);

            memoryCache.Set(cacheKey, cities, cacheExpiryOptions);
        }

        return cities;
    }
}