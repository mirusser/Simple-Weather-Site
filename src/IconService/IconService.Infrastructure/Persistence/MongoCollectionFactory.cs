using IconService.Application.Common.Interfaces.Persistence;
using IconService.Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IconService.Infrastructure.Persistence;

//TODO: maybe think about merging with MongoRepository
public class MongoCollectionFactory<TMongoDocument>
    : IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
{
    private readonly MongoSettings _settings;

    public MongoCollectionFactory(IOptions<MongoSettings> settings)
    {
        _settings = settings.Value;
    }

    public IMongoCollection<TMongoDocument> Get(string? collectionName = null)
    {
        collectionName = !string.IsNullOrEmpty(collectionName)
            ? collectionName
            : typeof(TMongoDocument).Name;

        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.Database);

        if (!database.ListCollectionNames().ToList().Any(c => c == collectionName))
        {
            database.CreateCollection(collectionName);
        }

        return database.GetCollection<TMongoDocument>(collectionName);
    }
}