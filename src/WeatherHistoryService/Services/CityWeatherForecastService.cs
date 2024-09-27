using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services;

public class CityWeatherForecastService : ICityWeatherForecastService
{
	private readonly IMongoCollection<CityWeatherForecastDocument> _cityWeatherForecastCollection;

	public CityWeatherForecastService(IMongoCollectionFactory<CityWeatherForecastDocument> mongoCollectionFactory)
	{
		_cityWeatherForecastCollection = mongoCollectionFactory.Create();
	}

	public async Task<IReadOnlyList<CityWeatherForecastDocument>> GetAllAsync(CancellationToken cancellationToken = default)
		=> await _cityWeatherForecastCollection.Find(_ => true).ToListAsync(cancellationToken);

	public async Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPaginationAsync(
		int numberOfEntities = 25,
		int pageNumber = 1,
		CancellationToken cancellationToken = default)
	{
		CityWeatherForecastPaginationDto cityWeatherForecastPaginationDto = new();

		cityWeatherForecastPaginationDto.NumberOfAllEntities = (int)await _cityWeatherForecastCollection
			.CountDocumentsAsync(c => c.Id != default, cancellationToken: cancellationToken);

		if (pageNumber >= 1 && numberOfEntities >= 1)
		{
			var howManyToSkip = pageNumber > 1 
				? numberOfEntities * (pageNumber - 1) 
				: 0;

			var cityWeatherForecastDocuments = _cityWeatherForecastCollection.AsQueryable()
				.Where(c => c.Id != default)
				.OrderByDescending(c => c.SearchDate)
				.Skip(howManyToSkip)
				.Take(numberOfEntities);

			cityWeatherForecastPaginationDto.WeatherForecastDocuments = await cityWeatherForecastDocuments
				.ToListAsync(cancellationToken);
		}

		return cityWeatherForecastPaginationDto;
	}

	public async Task<CityWeatherForecastDocument?> GetAsync(
		string id, 
		CancellationToken cancellationToken = default)
	{
		CityWeatherForecastDocument? cityWeatherForecastDocument = await _cityWeatherForecastCollection
			.Find(c => c.Id == id)
			.SingleOrDefaultAsync(cancellationToken);

		return cityWeatherForecastDocument;
	}

	public async Task<CityWeatherForecastDocument> CreateAsync(
		CityWeatherForecastDocument? cityWeatherForecast,
		CancellationToken cancellationToken = default)
	{
		if (cityWeatherForecast is null)
		{
			cityWeatherForecast = new();
		}

		await _cityWeatherForecastCollection
			.InsertOneAsync(cityWeatherForecast, cancellationToken);

		return cityWeatherForecast;
	}
}