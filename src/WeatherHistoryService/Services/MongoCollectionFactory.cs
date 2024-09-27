using System.Linq;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherHistoryService.Mongo;

namespace WeatherHistoryService.Services;

public class MongoCollectionFactory<TMongoDocument>(IOptions<MongoSettings> settings)
	: IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
{
    private readonly MongoSettings settings = settings.Value;

	public IMongoCollection<TMongoDocument> Create(string? collectionName = null)
    {
        collectionName = !string.IsNullOrEmpty(collectionName) 
            ? collectionName 
            : typeof(TMongoDocument).Name;

        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.Database);

        if (!database.ListCollectionNames().ToList().Any(c => c == collectionName))
        {
            database.CreateCollection(collectionName);
        }

        return database.GetCollection<TMongoDocument>(collectionName);
    }
}