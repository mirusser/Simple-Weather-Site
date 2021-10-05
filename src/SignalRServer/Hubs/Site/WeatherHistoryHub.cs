using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MQModels.WeatherHistory;
using SignalRServer.Mongo.Documents;
using SignalRServer.Services.Contracts;

namespace SignalRServer.Hubs.Site
{
    public class WeatherHistoryHub : Hub
    {
        private readonly IMongoCollection<WeatherHistoryConnection> _weatherHistoryConnectionCollection;

        public WeatherHistoryHub(IMongoCollectionFactory<WeatherHistoryConnection> mongoCollectionFactory)
        {
            _weatherHistoryConnectionCollection = mongoCollectionFactory.Create();
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connection Established, connection Id: {Context.ConnectionId}"); //TODO: log it maybe? (add logger)

            await _weatherHistoryConnectionCollection.InsertOneAsync(new WeatherHistoryConnection() { ConnectionId = Context.ConnectionId });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _weatherHistoryConnectionCollection.FindOneAndDeleteAsync(w => w.ConnectionId == Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}