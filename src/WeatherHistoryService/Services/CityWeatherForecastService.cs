using Convey.Persistence.MongoDB;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IReadOnlyList<CityWeatherForecastDocument>> GetAll()
        {
            return await _repository.FindAsync(c => c.Id != null);
        }

        public async Task<CityWeatherForecastDocument> GetAsync(string id)
        {
            CityWeatherForecastDocument cityWeatherForecastDocument = null;

            if (!string.IsNullOrEmpty(id))
            {
                var guidId = new Guid(Convert.FromBase64String(id.Trim()));
                cityWeatherForecastDocument = await _repository.GetAsync(guidId);
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
