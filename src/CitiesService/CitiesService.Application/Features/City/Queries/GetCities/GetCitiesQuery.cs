﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Common.Errors;
using CitiesService.Domain.Entities;
using Common.Infrastructure.Managers.Contracts;
using ErrorOr;
using IdentityModel.Client;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Features.City.Queries.GetCities;

public class GetCitiesQuery : IRequest<ErrorOr<GetCitiesResult>>
{
	public string CityName { get; set; } = null!;
	public int Limit { get; set; }
}

public class GetCitiesHandler(
		IGenericRepository<CityInfo> cityInfoRepo,
		IMapper mapper,
		ICacheManager cache,
		ILogger<GetCitiesHandler> logger)
	: IRequestHandler<GetCitiesQuery, ErrorOr<GetCitiesResult>>
{
	public async Task<ErrorOr<GetCitiesResult>> Handle(
		GetCitiesQuery request,
		CancellationToken cancellationToken)
	{
		request.CityName = request.CityName.TrimStart().TrimEnd();

		var cities = await GetCitiesByNameAsync(
			request.CityName,
			request.Limit,
			cancellationToken);

		return cities is null || cities.Count == 0
			? (ErrorOr<GetCitiesResult>)Errors.City.CityNotFound
			: (ErrorOr<GetCitiesResult>)new GetCitiesResult { Cities = cities };
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
			return citiesFromCache;
		}

		var cityInfoQuery = cityInfoRepo.FindAll(
			c => c.Name.Contains(cityName),
			orderByExpression: x => x.OrderBy(c => c.Id),
			takeNumberOfRows: limit);

		var citiesExist = await cityInfoQuery
			.AnyAsync(cancellationToken);

		if (!citiesExist)
		{
			return [];
		}

		var cityInfoList = await cityInfoQuery
			.GroupBy(x => x.Name)
			.Select(x => x.First())
			.ToListAsync(cancellationToken);

		var cities = mapper.Map<List<GetCityResult>>(cityInfoList);

		await cache.SetAsync(cacheKey, cities, cancellationToken);

		return cities;
	}
}