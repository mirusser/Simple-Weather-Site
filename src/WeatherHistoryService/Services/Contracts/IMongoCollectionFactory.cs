using MongoDB.Driver;

namespace WeatherHistoryService.Services.Contracts;

public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
{
    IMongoCollection<TMongoDocument> Create(string? collectionName = null);
}