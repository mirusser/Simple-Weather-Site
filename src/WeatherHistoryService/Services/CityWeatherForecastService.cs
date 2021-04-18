using Convey.Persistence.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services
{
    public class CityWeatherForecastService : ICityWeatherForecastService
    {
        private readonly IMongoRepository<CityWeatherForecastDocument, Guid> _repository;

        public CityWeatherForecastService(IMongoRepository<CityWeatherForecastDocument, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<CityWeatherForecastDocument> GetAsync(string id)
        {
            CityWeatherForecastDocument cityWeatherForecastDocument = null;

            if (!string.IsNullOrEmpty(id))
            {
                cityWeatherForecastDocument = await _repository.GetAsync(new Guid(id));
            }

            return cityWeatherForecastDocument;
        }

        public async Task<CityWeatherForecastDocument> CreateAsync(CityWeatherForecastDocument cityWeatherForecast)
        {
            if (cityWeatherForecast != null)
            {
                await _repository.AddAsync(cityWeatherForecast);
            }

            return cityWeatherForecast;
        }
    }
}
