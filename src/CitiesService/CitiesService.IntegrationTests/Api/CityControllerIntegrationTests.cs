using System.Net;
using System.Net.Http.Json;
using CitiesService.Contracts.City;
using CitiesService.Domain.Entities;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Xunit;

namespace CitiesService.IntegrationTests.Api;

/// <summary>
/// These tests validate the HTTP API end-to-end:
/// routing + model binding + mapping + mediator/validation + EF Core SQL provider.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class CityControllerIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task GetCitiesByName_WhenValidRequest_ReturnsCitiesFromDatabase()
    {
        var dbName = DbTestHelpers.CreateDatabaseName();
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await DbTestHelpers.MigrateAsync(db);
            db.CityInfos.AddRange(
                new CityInfo { CityId = 101m, Name = "London", CountryCode = "GB", Lat = 1, Lon = 2 },
                new CityInfo { CityId = 102m, Name = "Londonderry", CountryCode = "GB", Lat = 3, Lon = 4 },
                new CityInfo { CityId = 201m, Name = "Paris", CountryCode = "FR", Lat = 5, Lon = 6 });
            await db.SaveChangesAsync();
        }

        await using var factory = new CitiesApiFactory(cs);
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync(
            "/api/City/GetCitiesByName",
            new GetCitiesRequest(CityName: "Lon", Limit: 10));

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<GetCitiesResponse>();
        Assert.NotNull(body);
        Assert.NotNull(body!.Cities);
        Assert.Contains(body.Cities, c => c.Name == "London");
        Assert.Contains(body.Cities, c => c.Name == "Londonderry");
        Assert.DoesNotContain(body.Cities, c => c.Name == "Paris");
    }

    [SqlServerFact]
    public async Task GetCitiesByName_WhenInvalidRequest_ReturnsBadRequest()
    {
        var dbName = DbTestHelpers.CreateDatabaseName();
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await DbTestHelpers.MigrateAsync(db);
        }

        await using var factory = new CitiesApiFactory(cs);
        var client = factory.CreateClient();

        // CityName too short after trimming (validator should reject)
        var resp = await client.PostAsJsonAsync(
            "/api/City/GetCitiesByName",
            new GetCitiesRequest(CityName: " a ", Limit: 10));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [SqlServerFact]
    public async Task Metrics_AfterSampleTraffic_ReturnsPrometheusText()
    {
        var dbName = DbTestHelpers.CreateDatabaseName();
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await DbTestHelpers.MigrateAsync(db);
            db.CityInfos.Add(new CityInfo { CityId = 101m, Name = "London", CountryCode = "GB", Lat = 1, Lon = 2 });
            await db.SaveChangesAsync();
        }

        await using var factory = new CitiesApiFactory(cs);
        var client = factory.CreateClient();

        var traffic = await client.PostAsJsonAsync(
            "/api/City/GetCitiesByName",
            new GetCitiesRequest(CityName: "Lon", Limit: 10));
        Assert.Equal(HttpStatusCode.OK, traffic.StatusCode);

        var metrics = await client.GetStringAsync("/metrics");

        Assert.Contains("# TYPE", metrics);
        Assert.Contains("sws_cities_mediator_requests_total", metrics);
        Assert.Contains("sws_cities_cache_requests_total", metrics);
    }
}
