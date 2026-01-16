using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services;

public sealed class MongoIndexesHostedService(IMongoCollectionFactory<CityWeatherForecastDocument> factory)
    : IHostedService
{
    private readonly IMongoCollection<CityWeatherForecastDocument> collection = factory.Create();

    public async Task StartAsync(CancellationToken ct)
    {
        var index = new CreateIndexModel<CityWeatherForecastDocument>(
            Builders<CityWeatherForecastDocument>.IndexKeys.Ascending(x => x.EventId),
            new CreateIndexOptions { Unique = true });

        await collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}