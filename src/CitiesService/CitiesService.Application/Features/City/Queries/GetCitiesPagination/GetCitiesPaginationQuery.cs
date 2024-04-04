using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.ApplicationCommon.Interfaces.Persistance;
using CitiesService.Domain.Common.Errors;
using CitiesService.Domain.Entities;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CitiesService.Application.Features.City.Queries.GetCitiesPagination;

public class GetCitiesPaginationQuery : IRequest<ErrorOr<GetCitiesPaginationResult>>
{
    public int NumberOfCities { get; set; }
    public int PageNumber { get; set; }
}

public class GetCitiesPaginationHandler : IRequestHandler<GetCitiesPaginationQuery, ErrorOr<GetCitiesPaginationResult>>
{
    private readonly IGenericRepository<CityInfo> cityInfoRepo;
    private readonly IMapper mapper;
    private readonly IDistributedCache distributedCache;

    private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

    public GetCitiesPaginationHandler(
        IGenericRepository<CityInfo> cityInfoRepo,
        IMapper mapper,
        IDistributedCache distributedCache)
    {
        this.mapper = mapper;
        this.cityInfoRepo = cityInfoRepo;
        this.distributedCache = distributedCache;

        _distributedCacheEntryOptions = new DistributedCacheEntryOptions() //TODO: move to settings (?) or better move caching to new lib (?)
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));
    }

    public async Task<ErrorOr<GetCitiesPaginationResult>> Handle(
        GetCitiesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var citiesPaginationDto = await GetCitiesPaginationDto(request.NumberOfCities, request.PageNumber);

        if (citiesPaginationDto is null
            || citiesPaginationDto.Cities is null
            || citiesPaginationDto.Cities.Count == 0)
        {
            return Errors.City.CityNotFound;
        }

        return citiesPaginationDto;
    }

    private async Task<GetCitiesPaginationResult> GetCitiesPaginationDto(int numberOfCities = 25, int pageNumber = 1)
    {
        var cityInfoPaginationDto = await GetCitiesInfoPagination(numberOfCities, pageNumber);
        var cities = cityInfoPaginationDto.CityInfos is not null
            ? mapper.Map<List<GetCityResult>>(cityInfoPaginationDto.CityInfos)
            : new List<GetCityResult>();

        GetCitiesPaginationResult citiesPaginationDto = new()
        {
            Cities = cities,
            NumberOfAllCities = cityInfoPaginationDto.NumberOfAllCities
        };

        return citiesPaginationDto;
    }

    //uses redis caching
    private async Task<CityInfoPaginationDto> GetCitiesInfoPagination(int numberOfCities = 25, int pageNumber = 1)
    {
        var cacheKey = $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";
        string serializedCitiesInfoPagination;
        var redisCitiesPaginationDto = await distributedCache.GetAsync(cacheKey);

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
                cityInfoRepo
                .FindAll(orderByExpression: x => x.OrderBy(c => c.Name))
                .Count();

            if (pageNumber >= 1 && numberOfCities >= 1)
            {
                var howManyToSkip = pageNumber > 1 ? numberOfCities * (pageNumber - 1) : 0;

                result.CityInfos =
                    cityInfoRepo
                    .FindAll(orderByExpression: x => x.OrderBy(c => c.Name), takeNumberOfRows: numberOfCities, skipNumberOfRows: howManyToSkip)
                    .ToList();
            }

            serializedCitiesInfoPagination =
                JsonSerializer
                .Serialize(
                    result,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            redisCitiesPaginationDto = Encoding.UTF8.GetBytes(serializedCitiesInfoPagination);
            await distributedCache.SetAsync(cacheKey, redisCitiesPaginationDto, _distributedCacheEntryOptions);
        }

        return result;
    }
}