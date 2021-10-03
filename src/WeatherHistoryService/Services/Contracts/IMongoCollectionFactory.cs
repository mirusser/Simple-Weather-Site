using System.Threading.Tasks;
using MongoDB.Driver;

namespace WeatherHistoryService.Services
{
    public interface IMongoCollectionFactory<TMongoDocument> where TMongoDocument : class
    {
        IMongoCollection<TMongoDocument> Create(string? collectionName = null);
    }
}