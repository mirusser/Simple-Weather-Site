using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using MQModels.WeatherHistory;

namespace SignalRServer.Hubs.Site
{
    //TODO: improve saving connection, maybe save to external source or something, static class is pretty bad idea lol
    public static class Connection
    {
        public static List<string> ConnectionIds { get; set; } = new();
    }

    public class WeatherHistoryHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Connection Established, connection Id: {Context.ConnectionId}"); //TODO: log it maybe? (add logger)

            Connection.ConnectionIds.Add(Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Connection.ConnectionIds.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}