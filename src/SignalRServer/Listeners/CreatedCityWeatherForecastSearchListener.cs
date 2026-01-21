using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MQModels.WeatherHistory;
using SignalRServer.Hubs.Site;
using SignalRServer.Mongo.Documents;
using SignalRServer.Services.Contracts;
using SignalRServer.Settings;

namespace SignalRServer.Listeners;

public class CreatedCityWeatherForecastSearchListener(
    IOptions<HubMethods> options,
    IHubContext<WeatherHistoryHub> hubContext,
    IMongoCollectionFactory<WeatherHistoryConnection> mongoCollectionFactory,
    ILogger<CreatedCityWeatherForecastSearchListener> logger)
    : IConsumer<CreatedCityWeatherForecastSearch>
{
    private readonly HubMethods hubMethods = options.Value;

    private readonly IMongoCollection<WeatherHistoryConnection> weatherHistoryConnectionCollection =
        mongoCollectionFactory.Create();

    public async Task Consume(ConsumeContext<CreatedCityWeatherForecastSearch> context)
    {
        logger.LogInformation("Received {TypeOfMessage} message", nameof(CreatedCityWeatherForecastSearch));
        
        var weatherHistoryConnections
            = await weatherHistoryConnectionCollection.FindAsync(_ => true);

        if (weatherHistoryConnections is not null)
        {
            foreach (var weatherHistoryConnection in weatherHistoryConnections.ToList())
            {
                await hubContext.Clients
                    .Client(weatherHistoryConnection.ConnectionId)
                    .SendAsync(hubMethods.RefreshWeatherHistoryPage);
            }
        }
    }
}