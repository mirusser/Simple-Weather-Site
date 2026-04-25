using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Database;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Common.Infrastructure.Settings;
using Common.Testing.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CitiesService.IntegrationTests.HostedServices;

/// <summary>
/// Ensures the startup hosted service can (1) create the database if missing,
/// (2) apply migrations, and (3) invoke seeding.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class DbMigrateAndSeedHostedServiceIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task StartAsync_CreatesDb_AppliesMigrations_AndSeeds()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_startup");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "SqlServer",
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = cs,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        DbTestHelpers.AddSqlServerApplicationDbContext(services, cs);
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IDatabaseBootstrapper, SqlServerDatabaseBootstrapper>();
        services.AddScoped<ICitiesSeeder, TestCitiesSeeder>();
        services.AddSingleton(NullLogger<DbMigrateAndSeedHostedService>.Instance);
        services.AddSingleton(NullLogger<SqlServerDatabaseBootstrapper>.Instance);
        services.AddSingleton<DbMigrateAndSeedHostedService>();

        await using var provider = services.BuildServiceProvider();

        var sut = provider.GetRequiredService<DbMigrateAndSeedHostedService>();
        await sut.StartAsync(CancellationToken.None);

        await using var verifyDb = DbTestHelpers.CreateDbContext(cs);
        var pending = await DbTestHelpers.GetPendingMigrationsAsync(verifyDb);
        Assert.Empty(pending);

        var anyCity = await DbTestHelpers.AnyCitiesAsync(verifyDb);
        Assert.True(anyCity);
    }

    private sealed class TestCitiesSeeder(ICityRepository repo) : ICitiesSeeder
    {
        public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken)
        {
            if (await repo.CheckIfExistsAsync(_ => true, cancellationToken))
            {
                return true;
            }

            await repo.CreateAsync(new CityInfo
            {
                CityId = 1m,
                Name = "Seeded",
                CountryCode = "TS",
                Lat = 1,
                Lon = 2,
                State = null
            }, cancellationToken);
            await repo.SaveAsync(cancellationToken);
            return true;
        }
    }
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlDbMigrateAndSeedHostedServiceIntegrationTests(PostgreSqlFixture postgres)
{
    [PostgreSqlFact]
    public async Task StartAsync_CreatesDb_AppliesMigrations_AndSeeds()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_pg_startup");
        var cs = postgres.GetConnectionStringForDatabase(dbName);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "PostgreSql",
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = cs,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        DbTestHelpers.AddPostgreSqlApplicationDbContext(services, cs);
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IDatabaseBootstrapper, PostgreSqlDatabaseBootstrapper>();
        services.AddScoped<ICitiesSeeder, TestCitiesSeeder>();
        services.AddSingleton(NullLogger<DbMigrateAndSeedHostedService>.Instance);
        services.AddSingleton(NullLogger<PostgreSqlDatabaseBootstrapper>.Instance);
        services.AddSingleton<DbMigrateAndSeedHostedService>();

        await using var provider = services.BuildServiceProvider();

        var sut = provider.GetRequiredService<DbMigrateAndSeedHostedService>();
        await sut.StartAsync(CancellationToken.None);

        await using var verifyDb = DbTestHelpers.CreatePostgreSqlDbContext(cs);
        var pending = await DbTestHelpers.GetPendingMigrationsAsync(verifyDb);
        Assert.Empty(pending);

        var anyCity = await DbTestHelpers.AnyCitiesAsync(verifyDb);
        Assert.True(anyCity);
    }

    private sealed class TestCitiesSeeder(ICityRepository repo) : ICitiesSeeder
    {
        public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken)
        {
            if (await repo.CheckIfExistsAsync(_ => true, cancellationToken))
            {
                return true;
            }

            await repo.CreateAsync(new CityInfo
            {
                CityId = 1m,
                Name = "Seeded",
                CountryCode = "TS",
                Lat = 1,
                Lon = 2,
                State = null
            }, cancellationToken);
            await repo.SaveAsync(cancellationToken);
            return true;
        }
    }
}
