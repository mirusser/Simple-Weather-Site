using CitiesService.Domain.Entities;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using CitiesService.IntegrationTests.Infrastructure.SqlServer;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CitiesService.IntegrationTests.Grpc;

/// <summary>
/// Validates the gRPC service over real HTTP/2 (Kestrel) with a generated gRPC client.
/// This catches transport/proto/mapping issues that unit tests won't.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class CitiesGrpcIntegrationTests(SqlServerFixture sql)
{
    static CitiesGrpcIntegrationTests()
    {
        // Required for gRPC over h2c (HTTP/2 without TLS).
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    [SqlServerFact]
    public async Task GetCitiesPaginationInfo_ReturnsTotalCount()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_grpc");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await db.Database.MigrateAsync();
            db.CityInfos.AddRange(
                new CityInfo { CityId = 1m, Name = "A", CountryCode = "X", Lat = 0, Lon = 0 },
                new CityInfo { CityId = 2m, Name = "B", CountryCode = "X", Lat = 0, Lon = 0 });
            await db.SaveChangesAsync();
        }

        await using var host = new CitiesGrpcHostFixture(cs);
        await host.InitializeAsync();

        using var channel = GrpcChannel.ForAddress(host.Address, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });

        var client = new GrpcClient.Cities.CitiesClient(channel);
        var reply = await client.GetCitiesPaginationInfoAsync(new GrpcClient.CitiesPaginationInfoRequest());

        Assert.Equal(2, reply.NumberOfAllCities);
    }

    [SqlServerFact]
    public async Task GetCitiesStream_StreamsRequestedPage()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_grpc_stream");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await db.Database.MigrateAsync();
            db.CityInfos.AddRange(
                new CityInfo { CityId = 1m, Name = "Alpha", CountryCode = "X", Lat = 0, Lon = 0 },
                new CityInfo { CityId = 2m, Name = "Bravo", CountryCode = "X", Lat = 0, Lon = 0 },
                new CityInfo { CityId = 3m, Name = "Charlie", CountryCode = "X", Lat = 0, Lon = 0 });
            await db.SaveChangesAsync();
        }

        await using var host = new CitiesGrpcHostFixture(cs);
        await host.InitializeAsync();

        using var channel = GrpcChannel.ForAddress(host.Address, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });
        var client = new GrpcClient.Cities.CitiesClient(channel);

        var call = client.GetCitiesStream(new GrpcClient.CitiesStreamRequest { NumberOfCities = 2, PageNumber = 1 });
        var received = 0;
        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            received++;
        }

        Assert.Equal(2, received);
    }
}
