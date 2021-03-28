using Convey.Persistence.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Contracts.Repositories;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services
{
    public class CityWeatherForecastService : ICityWeatherForecastService
    {
        private readonly ICityWeatherForecastRepository _cityWeatherForecastRepository;

        public CityWeatherForecastService(ICityWeatherForecastRepository cityWeatherForecastRepository)
        {
            _cityWeatherForecastRepository = cityWeatherForecastRepository;
        }

        //TODO: add validation
        public async Task<CityWeatherForecastDocument> GetAsync(string id)
            => await _cityWeatherForecastRepository.GetAsync(id);

        //TODO: add validation
        public async Task<CityWeatherForecastDocument> CreateAsync(CityWeatherForecastDocument cityWeatherForecast)
            => await _cityWeatherForecastRepository.AddAsync(cityWeatherForecast);
    }
}
