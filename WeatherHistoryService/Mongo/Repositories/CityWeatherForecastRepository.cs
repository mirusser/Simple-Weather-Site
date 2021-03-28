using Convey.CQRS.Queries;
using Convey.Persistence.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Contracts.Repositories;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Mongo.Repositories
{
    public class CityWeatherForecastRepository : ICityWeatherForecastRepository
    {
        private readonly MongoSettings _settings;
        public IMongoCollection<CityWeatherForecastDocument> _cityWeatherForecastCollection;

        public CityWeatherForecastRepository(IOptions<MongoSettings> options)
        {
            _settings = options.Value;

            var client = new MongoClient(_settings.ConnectionString);
            var database = client.GetDatabase(_settings.Database);
            _cityWeatherForecastCollection = database.GetCollection<CityWeatherForecastDocument>(_settings.CityWeatherForecastsCollectionName);
        }

        public async Task<CityWeatherForecastDocument> GetAsync(string id)
            => await _cityWeatherForecastCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

        public Task<CityWeatherForecastDocument> GetAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CityWeatherForecastDocument>> FindAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<CityWeatherForecastDocument>> BrowseAsync<TQuery>(Expression<Func<CityWeatherForecastDocument, bool>> predicate, TQuery query) where TQuery : IPagedQuery
        {
            throw new NotImplementedException();
        }

        public async Task<CityWeatherForecastDocument> AddAsync(CityWeatherForecastDocument entity)
        {
            await _cityWeatherForecastCollection.InsertOneAsync(entity);

            return entity;
        }

        public Task UpdateAsync(CityWeatherForecastDocument entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(CityWeatherForecastDocument entity, Expression<Func<CityWeatherForecastDocument, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<CityWeatherForecastDocument, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
