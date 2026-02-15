using CitiesService.Infrastructure.Repositories;
using Common.Testing.DI;
using Common.Testing.TestDoubles;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CitiesService.IntegrationTests.Api;

public sealed class CitiesApiFactory(string connectionString) : WebApplicationFactory<CitiesService.Api.Controllers.CityController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
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
            // Integration tests should not download/seed the real city list.
            services.RemoveHostedServiceByTypeName(nameof(DbMigrateAndSeedHostedService));
            // Avoid RabbitMQ connectivity during API endpoint tests.
            services.RemoveHostedServiceByTypeName("MassTransitHostedService");

            // Avoid Redis connectivity by replacing the cache manager.
            services.RemoveServiceByTypeFullName(typeof(ICacheManager).FullName!);
            services.AddSingleton<ICacheManager, FakeCacheManager>();
        });
    }
}
