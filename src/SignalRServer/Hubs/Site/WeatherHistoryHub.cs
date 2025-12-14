using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using SignalRServer.Mongo.Documents;
using SignalRServer.Services.Contracts;

namespace SignalRServer.Hubs.Site;

public class WeatherHistoryHub(IMongoCollectionFactory<WeatherHistoryConnection> mongoCollectionFactory)
    : Hub
{
    private readonly IMongoCollection<WeatherHistoryConnection> weatherHistoryConnectionCollection = mongoCollectionFactory.Create();

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Connection Established, connection Id: {Context.ConnectionId}"); //TODO: log it maybe? (add logger)

        await weatherHistoryConnectionCollection.InsertOneAsync(new WeatherHistoryConnection() { ConnectionId = Context.ConnectionId });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await weatherHistoryConnectionCollection.FindOneAndDeleteAsync(w => w.ConnectionId == Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}