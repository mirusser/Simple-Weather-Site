using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services
{
    public class CityWeatherForecastService : ICityWeatherForecastService
    {
        private readonly IMongoCollection<CityWeatherForecastDocument> _cityWeatherForecastCollection;

        public CityWeatherForecastService(IMongoCollectionFactory<CityWeatherForecastDocument> mongoCollectionFactory)
        {
            _cityWeatherForecastCollection = mongoCollectionFactory.Create();
        }

        public async Task<IReadOnlyList<CityWeatherForecastDocument>> GetAll()
            => await _cityWeatherForecastCollection.Find(_ => true).ToListAsync();

        public async Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPagination(
            int numberOfEntities = 25,
            int pageNumber = 1)
        {
            CityWeatherForecastPaginationDto cityWeatherForecastPaginationDto = new();

            cityWeatherForecastPaginationDto.NumberOfAllEntities = (int)await _cityWeatherForecastCollection.CountDocumentsAsync(c => c.Id != default);

            if (pageNumber >= 1 && numberOfEntities >= 1)
            {
                var howManyToSkip = pageNumber > 1 ? numberOfEntities * (pageNumber - 1) : 0;

                var cityWeatherForecastDocuments =
                    _cityWeatherForecastCollection.AsQueryable().Where(c => c.Id != default).OrderByDescending(c => c.SearchDate).Skip(howManyToSkip).Take(numberOfEntities);

                cityWeatherForecastPaginationDto.WeatherForecastDocuments = cityWeatherForecastDocuments.ToList();
            }

            return cityWeatherForecastPaginationDto;
        }

        public async Task<CityWeatherForecastDocument?> GetAsync(string id)
        {
            CityWeatherForecastDocument? cityWeatherForecastDocument = null;

            cityWeatherForecastDocument =
                    await _cityWeatherForecastCollection.Find(c => c.Id == id).SingleOrDefaultAsync();

            //if (ObjectId.TryParse(id, out ObjectId objectId))
            //{
            //    cityWeatherForecastDocument =
            //        await _cityWeatherForecastCollection.Find(c => c.Id == objectId).SingleOrDefaultAsync();
            //}

            return cityWeatherForecastDocument;
        }

        public async Task<CityWeatherForecastDocument> CreateAsync(CityWeatherForecastDocument? cityWeatherForecast)
        {
            if (cityWeatherForecast is not null)
            {
                await _cityWeatherForecastCollection.InsertOneAsync(cityWeatherForecast);
            }
            else
            {
                cityWeatherForecast = new();
            }

            return cityWeatherForecast;
        }
    }
}