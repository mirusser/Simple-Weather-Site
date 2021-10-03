using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Services.Contracts
{
    public interface ICityWeatherForecastService
    {
        Task<IReadOnlyList<CityWeatherForecastDocument>> GetAll();

        Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPagination(int numberOfEntities = 25, int pageNumber = 1);

        Task<CityWeatherForecastDocument?> GetAsync(string id);

        Task<CityWeatherForecastDocument?> CreateAsync(CityWeatherForecastDocument? cityWeatherForecast);
    }
}