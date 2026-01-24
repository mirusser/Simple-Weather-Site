using IconService.Application.Common.Interfaces.Persistence;
using IconService.Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IconService.Infrastructure.Persistence;

public class MongoCollectionFactory<TMongoDocument>(
    IMongoClient client,
    IOptions<MongoSettings> settings)
    : IMongoCollectionFactory<TMongoDocument>
    where TMongoDocument : class
{
    private readonly MongoSettings settings = settings.Value;

    public IMongoCollection<TMongoDocument> Get(string? collectionName = null)
    {
        collectionName ??= typeof(TMongoDocument).Name;

        var database = client.GetDatabase(settings.Database);

        // optional: explicit creation
        var exists = database.ListCollectionNames().ToList().Any(c => c == collectionName);
        if (!exists)
        {
            database.CreateCollection(collectionName);
        }

        return database.GetCollection<TMongoDocument>(collectionName);
    }
}