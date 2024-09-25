using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Common.Errors;
using CitiesService.Domain.Entities;
using Common.Infrastructure.Managers.Contracts;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CitiesService.Application.Features.City.Queries.GetCitiesPagination;

public class GetCitiesPaginationQuery : IRequest<ErrorOr<GetCitiesPaginationResult>>
{
	public int NumberOfCities { get; set; }
	public int PageNumber { get; set; }
}

public class GetCitiesPaginationHandler(
	IGenericRepository<CityInfo> cityInfoRepo,
	IMapper mapper,
	ICacheManager cache) : IRequestHandler<GetCitiesPaginationQuery, ErrorOr<GetCitiesPaginationResult>>
{
	public async Task<ErrorOr<GetCitiesPaginationResult>> Handle(
		GetCitiesPaginationQuery request,
		CancellationToken cancellationToken)
	{
		var citiesPaginationDto = await GetCitiesPaginationDtoAsync(
			request.NumberOfCities,
			request.PageNumber,
			cancellationToken);

		if (citiesPaginationDto is null
			|| citiesPaginationDto.Cities is null
			|| citiesPaginationDto.Cities.Count == 0)
		{
			return Errors.City.CityNotFound;
		}

		return citiesPaginationDto;
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

		var cities = cityInfoPaginationDto.CityInfos is not null
			? mapper.Map<List<GetCityResult>>(cityInfoPaginationDto.CityInfos)
			: [];

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
		var cacheKey = $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";

		var (isSuccess, resultFromCache) = await cache
			.TryGetValueAsync<CityInfoPaginationDto>(cacheKey, cancellationToken);

		if (isSuccess && resultFromCache is not null)
		{
			return resultFromCache;
		}

		CityInfoPaginationDto? result = new()
		{
			NumberOfAllCities = await cityInfoRepo
				.FindAll(
					searchExpression: _ => true,
					orderByExpression: ci => ci.OrderBy(c => c.Name))
				.CountAsync(cancellationToken)
		};

		if (pageNumber >= 1 && numberOfCities >= 1)
		{
			var howManyToSkip = pageNumber > 1
				? numberOfCities * (pageNumber - 1)
				: 0;

			result.CityInfos = await cityInfoRepo
				.FindAll(
					searchExpression: _ => true,
					orderByExpression: x => x.OrderBy(c => c.Name),
					skipNumberOfRows: howManyToSkip,
					takeNumberOfRows: numberOfCities)
				.ToListAsync(cancellationToken);
		}

		await cache.SetAsync(cacheKey, result, cancellationToken);

		return result;
	}
}