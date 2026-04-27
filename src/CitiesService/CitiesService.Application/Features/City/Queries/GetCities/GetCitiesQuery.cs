using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Telemetry;
using CitiesService.Domain.Entities;
using Common.Infrastructure.Managers.Contracts;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Features.City.Queries.GetCities;

public class GetCitiesQuery : IRequest<Result<GetCitiesResult>>
{
    public string CityName { get; set; } = null!;
    public int Limit { get; init; }
}

public class GetCitiesHandler(
    IGenericRepository<CityInfo> cityInfoRepo,
    ICacheManager cache,
    ILogger<GetCitiesHandler> logger)
    : IRequestHandler<GetCitiesQuery, Result<GetCitiesResult>>
{
    public async Task<Result<GetCitiesResult>> Handle(
        GetCitiesQuery request,
        CancellationToken cancellationToken)
    {
        request.CityName = request.CityName.TrimStart().TrimEnd();

        var cities = await GetCitiesByNameAsync(
            request.CityName,
            request.Limit,
            cancellationToken);
        CitiesTelemetry.RecordReturnedCities(
            CitiesTelemetryConventions.Operations.GetCitiesByName,
            cities?.Count ?? 0,
            CitiesTelemetryConventions.ResultValues.Success);

        return Result<GetCitiesResult>.Ok(new GetCitiesResult
        {
            Cities = cities ?? []
        });
    }

    private async Task<List<GetCityResult>?> GetCitiesByNameAsync(
        string cityName,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            return [];
        }

        var cacheKey = $"GetCitiesByName-{nameof(cityName)}-{cityName}-{nameof(limit)}-{limit}";

        var (isSuccess, citiesFromCache) = await cache
            .TryGetValueAsync<List<GetCityResult>?>(cacheKey, cancellationToken);

        if (isSuccess && citiesFromCache is not null)
        {
            CitiesTelemetry.RecordCacheRequest(
                CitiesTelemetryConventions.Operations.GetCitiesByName,
                CitiesTelemetryConventions.CacheResults.Hit);
            return citiesFromCache;
        }

        CitiesTelemetry.RecordCacheRequest(
            CitiesTelemetryConventions.Operations.GetCitiesByName,
            CitiesTelemetryConventions.CacheResults.Miss);

        var cityInfoList = await cityInfoRepo.ListAsync(
            c => c.Name.Contains(cityName),
            orderByExpression: x => x.OrderBy(c => c.Id),
            takeNumberOfRows: limit,
            cancellationToken: cancellationToken);

        if (cityInfoList.Count == 0)
        {
            return [];
        }

        cityInfoList = cityInfoList
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();

        var cities = cityInfoList
            .Select(c => new GetCityResult()
            {
                Id = c.CityId,
                Name = c.Name,
                Country = c.CountryCode,
                Coord = new Coord() { Lat = c.Lat, Lon = c.Lon },
                State = c.State
            })
            .ToList();

        await cache.SetAsync(cacheKey, cities, cancellationToken);

        return cities;
    }
}
