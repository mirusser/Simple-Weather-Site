using MongoDB.Driver;

namespace IconService.Application.Common.Interfaces.Persistence;

public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
{
    IMongoCollection<TMongoDocument> Get(string? collectionName = null);
}