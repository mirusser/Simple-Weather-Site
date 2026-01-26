using Common.Presentation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRServer.Hubs.Site;
using SignalRServer.Listeners;
using SignalRServer.Services;
using SignalRServer.Services.Contracts;
using SignalRServer.Settings;
using Common.Application.HealthChecks;
using Common.Contracts.HealthCheck;
using Common.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();

    builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection(nameof(MongoSettings)));
    builder.Services.Configure<HubMethods>(builder.Configuration.GetSection(nameof(HubMethods)));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policyBuilder =>
            {
                policyBuilder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed((_) => true)
                    .AllowCredentials();
            });
    });

    builder.Services.AddSignalR();

    builder.Services.AddMassTransit(config =>
    {
        RabbitMQSettings rabbitMqSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMqSettings);

        config.AddConsumer<CreatedCityWeatherForecastSearchListener>();
        config.SetKebabCaseEndpointNameFormatter();

        config.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitMqSettings.Host);
            cfg.ConfigureEndpoints(ctx);
        });
    });

    builder.Services.AddOptions<MassTransitHostOptions>()
        .Configure(options =>
        {
            // if specified, waits until the bus is started before
            // returning from IHostedService.StartAsync
            // default is false
            options.WaitUntilStarted = true;

            // if specified, limits the wait time when starting the bus
            //options.StartTimeout = TimeSpan.FromSeconds(10);

            // if specified, limits the wait time when stopping the bus
            //options.StopTimeout = TimeSpan.FromSeconds(30);
        });

    MongoSettings mongoSettings = new();
    builder.Configuration.GetSection(nameof(MongoSettings)).Bind(mongoSettings);

    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(mongoSettings.ConnectionString));

    builder.Services.AddSingleton(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(mongoSettings.Database);
    });
    
    builder.Services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
    
    builder.Services
        .AddCommonHealthChecks()
        .AddMongoDb(
            clientFactory: sp => sp.GetRequiredService<IMongoClient>(),
            name: "Mongo health check",
            failureStatus: HealthStatus.Unhealthy,
            tags: [HealthChecksTags.Ready, HealthChecksTags.Database]);
}

var app = builder.Build();
{
    app.UseDefaultExceptionHandler();
    app.UseDefaultScalar();

    app.UseCors("AllowAll");

    app.UseHttpsRedirection();
    app.UseRouting();

    app.MapHub<WeatherHistoryHub>($"/{nameof(WeatherHistoryHub)}");

    //app.UseSignalR(routes =>
    //{
    //    routes.MapHub<TestHub>("/test");
    //});
    
    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();