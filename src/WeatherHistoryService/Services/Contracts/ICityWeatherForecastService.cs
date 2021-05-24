using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Services.Contracts
{
    public interface ICityWeatherForecastService
    {
        Task<IReadOnlyList<CityWeatherForecastDocument>> GetAll();
        Task<CityWeatherForecastDocument> GetAsync(string id);
        Task<CityWeatherForecastDocument> CreateAsync(CityWeatherForecastDocument cityWeatherForecast);
    }
}
