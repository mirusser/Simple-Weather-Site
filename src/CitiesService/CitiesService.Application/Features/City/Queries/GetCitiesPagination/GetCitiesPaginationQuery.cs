using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Common.Errors;
using CitiesService.Domain.Entities;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CitiesService.Application.Features.City.Queries.GetCitiesPagination;

public class GetCitiesPaginationQuery : IRequest<ErrorOr<GetCitiesPaginationResult>>
{
	public int NumberOfCities { get; set; }
	public int PageNumber { get; set; }
}

public class GetCitiesPaginationHandler(
	IGenericRepository<CityInfo> cityInfoRepo,
	IMapper mapper,
	IDistributedCache distributedCache,
	JsonSerializerOptions jsonSerializerOptions) : IRequestHandler<GetCitiesPaginationQuery, ErrorOr<GetCitiesPaginationResult>>
{
	private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions =
		new DistributedCacheEntryOptions() //TODO: move to settings (?) or better move caching to new lib (?)
			.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
			.SetSlidingExpiration(TimeSpan.FromMinutes(2));

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

	// TODO: uses redis caching
	private async Task<CityInfoPaginationDto> GetCitiesInfoPaginationAsync(
		int numberOfCities = 25,
		int pageNumber = 1,
		CancellationToken cancellationToken = default)
	{
		var cacheKey = $"GetCitiesPagination-{nameof(numberOfCities)}-{numberOfCities}-{nameof(pageNumber)}-{pageNumber}";
		string serializedCitiesInfoPagination;
		var redisCitiesPaginationDto = await distributedCache.GetAsync(cacheKey, cancellationToken);

		CityInfoPaginationDto? result = new();

		if (redisCitiesPaginationDto is not null)
		{
			serializedCitiesInfoPagination = Encoding.UTF8.GetString(redisCitiesPaginationDto);
			result = JsonSerializer.Deserialize<CityInfoPaginationDto>(
				serializedCitiesInfoPagination,
				jsonSerializerOptions);

			result ??= new();

			return result;
		}

		result.NumberOfAllCities = await cityInfoRepo
			.FindAll(
				searchExpression: _ => true,
				orderByExpression: x => x.OrderBy(c => c.Name))
			.CountAsync(cancellationToken);

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

		serializedCitiesInfoPagination = JsonSerializer.Serialize(
			result,
			jsonSerializerOptions);

		redisCitiesPaginationDto = Encoding.UTF8.GetBytes(serializedCitiesInfoPagination);

		await distributedCache.SetAsync(
			cacheKey,
			redisCitiesPaginationDto,
			_distributedCacheEntryOptions,
			cancellationToken);

		return result;
	}
}