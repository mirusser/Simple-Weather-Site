using MongoDB.Driver;

namespace IconService.Services
{
    public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
    {
        IMongoCollection<TMongoDocument> Get(string? collectionName = null);
    }
}