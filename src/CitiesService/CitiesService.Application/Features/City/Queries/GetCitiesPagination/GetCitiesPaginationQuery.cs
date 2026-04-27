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

namespace CitiesService.Application.Features.City.Queries.GetCitiesPagination;

public class GetCitiesPaginationQuery : IRequest<Result<GetCitiesPaginationResult>>
{
    public int NumberOfCities { get; init; }
    public int PageNumber { get; init; }
}

public class GetCitiesPaginationHandler(
    IGenericRepository<CityInfo> cityInfoRepo,
    ICacheManager cache) : IRequestHandler<GetCitiesPaginationQuery, Result<GetCitiesPaginationResult>>
{
    public async Task<Result<GetCitiesPaginationResult>> Handle(
        GetCitiesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var citiesPaginationDto = await GetCitiesPaginationDtoAsync(
            request.NumberOfCities,
            request.PageNumber,
            cancellationToken);
        CitiesTelemetry.RecordReturnedCities(
            CitiesTelemetryConventions.Operations.GetCitiesPagination,
            citiesPaginationDto.Cities.Count,
            CitiesTelemetryConventions.ResultValues.Success);

        return Result<GetCitiesPaginationResult>.Ok(citiesPaginationDto);
    }

    private async Task<GetCitiesPaginationResult> GetCitiesPaginationDtoAsync(
        int numberOfCities = 25,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var cityInfoPaginationDto = await GetCitiesInfoPaginationAsync(
            numberOfCities,
            pageNumber,
            cancellationToken);

        var cities = cityInfoPaginationDto.CityInfos
            .Select(c => new GetCityResult()
            {
                Id = c.Id,
                Name = c.Name,
                Coord = new Coord() { Lat = c.Lat, Lon = c.Lon },
                State = c.State,
                Country = c.CountryCode
            })
            .ToList();

        GetCitiesPaginationResult citiesPaginationDto = new()
        {
            Cities = cities,
            NumberOfAllCities = cityInfoPaginationDto.NumberOfAllCities
        };

        return citiesPaginationDto;
    }

    private async Task<CityInfoPaginationDto> GetCitiesInfoPaginationAsync(
        int numberOfCities = 25,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var cacheKey =
            $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";

        var (isSuccess, resultFromCache) = await cache
            .TryGetValueAsync<CityInfoPaginationDto>(cacheKey, cancellationToken);

        if (isSuccess && resultFromCache is not null)
        {
            CitiesTelemetry.RecordCacheRequest(
                CitiesTelemetryConventions.Operations.GetCitiesPagination,
                CitiesTelemetryConventions.CacheResults.Hit);
            return resultFromCache;
        }

        CitiesTelemetry.RecordCacheRequest(
            CitiesTelemetryConventions.Operations.GetCitiesPagination,
            CitiesTelemetryConventions.CacheResults.Miss);

        CityInfoPaginationDto? result = new()
        {
            NumberOfAllCities = await cityInfoRepo.CountAsync(_ => true, cancellationToken)
        };

        if (pageNumber >= 1 && numberOfCities >= 1)
        {
            var howManyToSkip = pageNumber > 1
                ? numberOfCities * (pageNumber - 1)
                : 0;

            result.CityInfos = await cityInfoRepo.ListAsync(
                searchExpression: _ => true,
                orderByExpression: x => x.OrderBy(c => c.Name),
                skipNumberOfRows: howManyToSkip,
                takeNumberOfRows: numberOfCities,
                cancellationToken: cancellationToken);
        }

        await cache.SetAsync(cacheKey, result, cancellationToken);

        return result;
    }
}
