using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MQModels.WeatherHistory;
using SignalRServer.Hubs.Site;
using SignalRServer.Mongo.Documents;
using SignalRServer.Services.Contracts;
using SignalRServer.Settings;

namespace SignalRServer.Listeners;

public class CreatedCityWeatherForecastSearchListener : IConsumer<CreatedCityWeatherForecastSearch>
{
    private readonly HubMethods _hubMethods;
    private readonly IHubContext<WeatherHistoryHub> _hubContext;

    private readonly IMongoCollection<WeatherHistoryConnection> _weatherHistoryConnectionCollection;

    public CreatedCityWeatherForecastSearchListener(
        IOptions<HubMethods> options,
        IHubContext<WeatherHistoryHub> hubContext,
        IMongoCollectionFactory<WeatherHistoryConnection> mongoCollectionFactory)
    {
        _hubMethods = options.Value;
        _hubContext = hubContext;
        _weatherHistoryConnectionCollection = mongoCollectionFactory.Create();
    }

    public async Task Consume(ConsumeContext<CreatedCityWeatherForecastSearch> context)
    {
        var weatherHistoryConnections = await _weatherHistoryConnectionCollection.FindAsync(_ => true);

        if (weatherHistoryConnections is not null)
        {
            foreach (var weatherHistoryConnection in weatherHistoryConnections.ToList())
            {
                await _hubContext.Clients.Client(weatherHistoryConnection.ConnectionId).SendAsync(_hubMethods.RefreshWeatherHistoryPage);
            }
        }
    }
}