using Convey.Persistence.MongoDB;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;
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

        public async Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPagination(
            int numberOfEntities = 25, 
            int pageNumber = 1)
        {
            CityWeatherForecastPaginationDto cityWeatherForecastPaginationDto = new();

            cityWeatherForecastPaginationDto.NumberOfAllEntities = (await _repository.FindAsync(c => c.Id != default)).Count;

            if (pageNumber >= 1 && numberOfEntities >= 1)
            {
                var howManyToSkip = pageNumber > 1 ? numberOfEntities * (pageNumber - 1) : 0;

                var cityWeatherForecastDocuments = (await _repository.FindAsync(c => c.Id != default)).OrderByDescending(c => c.SearchDate).Skip(howManyToSkip).Take(numberOfEntities);

                cityWeatherForecastPaginationDto.WeatherForecastDocuments = cityWeatherForecastDocuments.ToList();
            }

            return cityWeatherForecastPaginationDto;
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
