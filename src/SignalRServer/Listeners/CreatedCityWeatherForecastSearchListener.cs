using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using SignalRServer.Hubs.Site;

namespace SignalRServer.Listeners
{
    public class CreatedCityWeatherForecastSearchListener : IConsumer<CreatedCityWeatherForecastSearchListener>
    {
        private readonly IHubContext<WeatherHistoryHub> _hubContext;

        public CreatedCityWeatherForecastSearchListener(IHubContext<WeatherHistoryHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<CreatedCityWeatherForecastSearchListener> context)
        {
            foreach (var connection in Connection.ConnectionIds)
            {
                await _hubContext.Clients.Client(connection).SendAsync("RefreshPageMessage"); //TODO: add to settings
            }
        }
    }
}