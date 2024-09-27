using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Services.Contracts;

public interface ICityWeatherForecastService
{
    Task<IReadOnlyList<CityWeatherForecastDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPaginationAsync(int numberOfEntities = 25, int pageNumber = 1, CancellationToken cancellationToken = default);

    Task<CityWeatherForecastDocument?> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<CityWeatherForecastDocument> CreateAsync(CityWeatherForecastDocument? cityWeatherForecast, CancellationToken cancellationToken = default);
}