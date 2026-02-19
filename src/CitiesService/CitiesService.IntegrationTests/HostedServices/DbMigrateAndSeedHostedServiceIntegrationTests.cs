using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Common.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
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
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = cs,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped<ApplicationDbContext>();
        services.AddScoped<ICitiesSeeder, TestCitiesSeeder>();
        services.AddSingleton(NullLogger<DbMigrateAndSeedHostedService>.Instance);
        services.AddSingleton<DbMigrateAndSeedHostedService>();

        await using var provider = services.BuildServiceProvider();

        var sut = provider.GetRequiredService<DbMigrateAndSeedHostedService>();
        await sut.StartAsync(CancellationToken.None);

        await using var verifyDb = DbTestHelpers.CreateDbContext(cs);
        var pending = await verifyDb.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);

        var anyCity = await verifyDb.CityInfos.AnyAsync();
        Assert.True(anyCity);
    }

    private sealed class TestCitiesSeeder(ApplicationDbContext db) : ICitiesSeeder
    {
        public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken)
        {
            if (await db.CityInfos.AnyAsync(cancellationToken))
            {
                return true;
            }

            db.CityInfos.Add(new CityInfo
            {
                CityId = 1m,
                Name = "Seeded",
                CountryCode = "TS",
                Lat = 1,
                Lon = 2,
                State = null
            });
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
