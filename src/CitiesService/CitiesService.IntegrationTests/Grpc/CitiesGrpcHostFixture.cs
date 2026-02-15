using System.Net;
using CitiesService.Application;
using CitiesService.Infrastructure;
using CitiesService.Infrastructure.Repositories;
using Common.Testing.DI;
using Common.Testing.TestDoubles;
using Common.Application.Mapping;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CitiesService.IntegrationTests.Grpc;

/// <summary>
/// Hosts the gRPC service on Kestrel (real HTTP/2) so we can use a real gRPC client.
/// </summary>
public sealed class CitiesGrpcHostFixture(string connectionString) : IAsyncLifetime, IAsyncDisposable
{
    private WebApplication? app;

    public Uri Address { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, 0, lo => lo.Protocols = HttpProtocols.Http2);
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = connectionString,
            [$"ConnectionStrings:{nameof(ConnectionStrings.RedisConnection)}"] = "localhost:6379",
            ["RabbitMQSettings:Host"] = "localhost",
            ["ResiliencePipelines:Default:Name"] = "default",
            ["ResiliencePipelines:Health:Name"] = "health",
        });

        // Matches CitiesGrpcService.Program: required for JSON options, common services, etc.
        builder.AddCommonPresentationLayer();

        builder.Services.AddGrpc(o =>
        {
            o.EnableDetailedErrors = true;
        });
        builder.Services
            .AddApplicationLayer(builder.Configuration)
            .AddInfrastructureLayer(builder.Configuration);

        builder.Services.AddHttpClient();

        builder.Services.AddMappings(typeof(CitiesGrpcService.Services.CitiesService).Assembly);

        // Disable hosted services that are not part of these tests.
        builder.Services.RemoveHostedServiceByTypeName(nameof(DbMigrateAndSeedHostedService));
        builder.Services.RemoveHostedServiceByTypeName("MassTransitHostedService");

        // Avoid Redis connectivity.
        builder.Services.RemoveServiceByTypeFullName(typeof(ICacheManager).FullName!);
        builder.Services.AddSingleton<ICacheManager, FakeCacheManager>();

        app = builder.Build();
        app.MapGrpcService<CitiesGrpcService.Services.CitiesService>();
        await app.StartAsync();

        var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses;
        Address = new Uri(addresses.Single());
    }

    public async Task DisposeAsync()
    {
        if (app is not null)
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync() => await DisposeAsync();
}
