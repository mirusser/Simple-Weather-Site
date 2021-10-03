using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherHistoryService.Mongo;

namespace WeatherHistoryService.Services
{
    public class MongoCollectionFactory<TMongoDocument> : IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
    {
        private readonly MongoSettings _settings;

        public MongoCollectionFactory(IOptions<MongoSettings> settings)
        {
            _settings = settings.Value;
        }

        public IMongoCollection<TMongoDocument> Create(string? collectionName = null)
        {
            collectionName = !string.IsNullOrEmpty(collectionName) ? collectionName : _settings.CollectionName;
            var client = new MongoClient(_settings.ConnectionString);
            var database = client.GetDatabase(_settings.Database);

            if (!database.ListCollectionNames().ToList().Any(c => c == collectionName))
            {
                database.CreateCollection(collectionName);
            }

            return database.GetCollection<TMongoDocument>(collectionName);
        }
    }
}