using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Testing.DI;
using Common.Testing.PostgreSql;
using Common.Testing.SqlServer;
using Common.Testing.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CitiesService.IntegrationTests.Startup;

[Collection(SqlServerCollection.Name)]
public class SqlServerApiStartupDatabaseWorkflowIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task ApiStartup_CreatesDatabase_AppliesMigrations_AndRunsSeeder()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_api_startup");
        var cs = sql.GetConnectionStringForDatabase(dbName);
        var probe = new StartupSeederProbe();

        using var factory = new CitiesApiStartupFactory("SqlServer", cs, probe);
        using var client = factory.CreateClient();

        await using var verifyDb = DbTestHelpers.CreateSqlServerDbContext(cs);
        Assert.Empty(await DbTestHelpers.GetPendingMigrationsAsync(verifyDb));
        Assert.True(await DbTestHelpers.AnyCitiesAsync(verifyDb));
        Assert.Equal(1, probe.CallCount);
    }
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlApiStartupDatabaseWorkflowIntegrationTests(PostgreSqlFixture postgres)
{
    [PostgreSqlFact]
    public async Task ApiStartup_CreatesDatabase_AppliesMigrations_AndRunsSeeder()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_pg_api_startup");
        var cs = postgres.GetConnectionStringForDatabase(dbName);
        var probe = new StartupSeederProbe();

        using var factory = new CitiesApiStartupFactory("PostgreSql", cs, probe);
        using var client = factory.CreateClient();

        await using var verifyDb = DbTestHelpers.CreatePostgreSqlDbContext(cs);
        Assert.Empty(await DbTestHelpers.GetPendingMigrationsAsync(verifyDb));
        Assert.True(await DbTestHelpers.AnyCitiesAsync(verifyDb));
        Assert.Equal(1, probe.CallCount);
    }
}

file sealed class CitiesApiStartupFactory(
    string provider,
    string connectionString,
    StartupSeederProbe probe)
    : WebApplicationFactory<CitiesService.Api.Controllers.CityController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("Database:Provider", provider);
        builder.UseSetting($"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}", connectionString);

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = provider,
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = connectionString,
                [$"ConnectionStrings:{nameof(ConnectionStrings.RedisConnection)}"] = "localhost:6379",
                ["RabbitMQSettings:Host"] = "localhost",
                ["FileUrlsAndPaths:CityListFileUrl"] = "http://localhost/city.list.json.gz",
                ["FileUrlsAndPaths:CompressedCityListFilePath"] = "./ignored.gz",
                ["FileUrlsAndPaths:DecompressedCityListFilePath"] = "./ignored.json",
                ["ResiliencePipelines:Default:Name"] = "default",
                ["ResiliencePipelines:Health:Name"] = "health",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveHostedServiceByTypeName("MassTransitHostedService");

            services.RemoveServiceByTypeFullName(typeof(ICacheManager).FullName!);
            services.AddSingleton<ICacheManager, FakeCacheManager>();

            services.RemoveAll<ICitiesSeeder>();
            services.AddSingleton(probe);
            services.AddScoped<ICitiesSeeder, StartupTestCitiesSeeder>();
        });
    }
}

file sealed class StartupTestCitiesSeeder(
    ICityRepository repo,
    StartupSeederProbe probe) : ICitiesSeeder
{
    public async Task<bool> SeedIfEmptyAsync(CancellationToken cancellationToken)
    {
        probe.MarkCalled();

        if (await repo.CheckIfExistsAsync(_ => true, cancellationToken))
        {
            return true;
        }

        await repo.CreateAsync(new CityInfo
        {
            CityId = 42_4242m,
            Name = "StartupSeeded",
            CountryCode = "TS",
            State = null,
            Lat = 1,
            Lon = 2
        }, cancellationToken);

        await repo.SaveAsync(cancellationToken);
        return true;
    }
}

file sealed class StartupSeederProbe
{
    private int callCount;

    public int CallCount => Volatile.Read(ref callCount);

    public void MarkCalled()
        => Interlocked.Increment(ref callCount);
}
