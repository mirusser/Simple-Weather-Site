using CitiesService.Application.Common.Exceptions;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Entities.Dtos;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Database;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Common.Infrastructure.Settings;
using Common.Testing.PostgreSql;
using Common.Testing.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CitiesService.IntegrationTests.Database;

[Collection(SqlServerCollection.Name)]
public class SqlServerCityConcurrencyIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task PatchAsync_WithStaleRowVersion_ThrowsApplicationConcurrencyException()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_concurrency");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        var cityId = await CityConcurrencyTestHelpers.CreateDatabaseAndCityAsync(
            DbTestHelpers.CreateSqlServerDbContext,
            cs);

        await CityConcurrencyTestHelpers.AssertStaleRowVersionThrowsAsync(
            DbTestHelpers.CreateSqlServerDbContext,
            cs,
            cityId);
    }
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlCityConcurrencyIntegrationTests(PostgreSqlFixture postgres)
{
    [PostgreSqlFact]
    public async Task PatchAsync_WithStaleRowVersion_ThrowsApplicationConcurrencyException()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_pg_concurrency");
        var cs = postgres.GetConnectionStringForDatabase(dbName);

        await CityConcurrencyTestHelpers.EnsurePostgreSqlDatabaseExistsAsync(cs);

        var cityId = await CityConcurrencyTestHelpers.CreateDatabaseAndCityAsync(
            DbTestHelpers.CreatePostgreSqlDbContext,
            cs);

        await CityConcurrencyTestHelpers.AssertStaleRowVersionThrowsAsync(
            DbTestHelpers.CreatePostgreSqlDbContext,
            cs,
            cityId);
    }
}

file static class CityConcurrencyTestHelpers
{
    public static async Task<int> CreateDatabaseAndCityAsync(
        Func<string, ApplicationDbContext> createDb,
        string connectionString)
    {
        await using var db = createDb(connectionString);
        await DbTestHelpers.MigrateAsync(db);

        var city = new CityInfo
        {
            CityId = 12345m,
            Name = "Original",
            CountryCode = "TS",
            State = null,
            Lat = 1,
            Lon = 2
        };

        db.CityInfos.Add(city);
        await db.SaveChangesAsync();

        Assert.NotEmpty(city.RowVersion);

        return city.Id;
    }

    public static async Task AssertStaleRowVersionThrowsAsync(
        Func<string, ApplicationDbContext> createDb,
        string connectionString,
        int cityId)
    {
        byte[] originalRowVersion;
        byte[] updatedRowVersion;

        await using (var db = createDb(connectionString))
        {
            var city = await db.CityInfos.SingleAsync(c => c.Id == cityId);
            originalRowVersion = city.RowVersion.ToArray();

            city.Name = "UpdatedOnce";
            await db.SaveChangesAsync();

            updatedRowVersion = city.RowVersion.ToArray();
        }

        Assert.NotEmpty(updatedRowVersion);
        Assert.False(
            originalRowVersion.SequenceEqual(updatedRowVersion),
            "RowVersion should change after a successful update.");

        await using var staleDb = createDb(connectionString);
        var repo = new CityRepository(staleDb);

        await Assert.ThrowsAsync<PersistenceConcurrencyException>(() =>
            repo.PatchAsync(new CityPatchDto(
                Id: cityId,
                CityId: null,
                Name: "StaleUpdate",
                State: null,
                CountryCode: null,
                Lon: null,
                Lat: null,
                RowVersion: originalRowVersion)));
    }

    public static async Task EnsurePostgreSqlDatabaseExistsAsync(string connectionString)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = connectionString,
            })
            .Build();

        var bootstrapper = new PostgreSqlDatabaseBootstrapper(
            config,
            NullLogger<PostgreSqlDatabaseBootstrapper>.Instance);

        await bootstrapper.EnsureDatabaseExistsAsync(CancellationToken.None);
    }
}
