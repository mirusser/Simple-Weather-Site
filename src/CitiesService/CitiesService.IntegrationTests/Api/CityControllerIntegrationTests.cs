using System.Net;
using System.Net.Http.Json;
using CitiesService.Contracts.City;
using CitiesService.Domain.Entities;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
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
            await db.Database.MigrateAsync();
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
            await db.Database.MigrateAsync();
        }

        await using var factory = new CitiesApiFactory(cs);
        var client = factory.CreateClient();

        // CityName too short after trimming (validator should reject)
        var resp = await client.PostAsJsonAsync(
            "/api/City/GetCitiesByName",
            new GetCitiesRequest(CityName: " a ", Limit: 10));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
