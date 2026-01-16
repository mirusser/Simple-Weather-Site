using System;
using System.Reflection;
using Common.Application.Mapping;
using Common.Presentation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherHistoryService.Listeners;
using WeatherHistoryService.Services;
using WeatherHistoryService.Services.Contracts;
using WeatherHistoryService.Settings;
using Common.Application.HealthChecks;
using Common.Mediator.DependencyInjection;
using Common.Shared;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
{
    var executingAssembly = Assembly.GetExecutingAssembly();

    builder.AddCommonPresentationLayer();

    builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection(nameof(MongoSettings)));

    builder.Services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddMappings(executingAssembly);

    MongoSettings mongoSettings = new();
    builder.Configuration.GetSection(nameof(MongoSettings)).Bind(mongoSettings);

    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(mongoSettings.ConnectionString));

    builder.Services.AddSingleton(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(mongoSettings.Database);
    });
    
    builder.Services.AddHostedService<MongoIndexesHostedService>();

    builder.Services.AddMassTransit(config =>
    {
        config.AddConsumer<GotWeatherForecastListener>();
        config.SetKebabCaseEndpointNameFormatter();

        // Mongo outbox config (used by consumer outbox/inbox too)
        config.AddMongoDbOutbox(o =>
        {
            o.QueryDelay = TimeSpan.FromSeconds(mongoSettings.OutboxSettings.QueryDelaySeconds);
            o.ClientFactory(sp => sp.GetRequiredService<IMongoClient>());
            o.DatabaseFactory(sp => sp.GetRequiredService<IMongoDatabase>());

            // how long to remember MessageId to prevent duplicates
            o.DuplicateDetectionWindow =
                TimeSpan.FromSeconds(mongoSettings.OutboxSettings.DuplicateDetectionWindowSeconds);
        });

        // Apply to all receive endpoints:
        config.AddConfigureEndpointsCallback((context, name, cfg) =>
        {
            // delayed redelivery (for transient issues)
            cfg.UseDelayedRedelivery(r =>
                r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2)));

            // immediate retry (fast transient failures)
            cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 2000));

            // consumer outbox/inbox using MongoDB
            cfg.UseMongoDbOutbox(context);
        });

        RabbitMqSettings rabbitMqSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMqSettings)).Bind(rabbitMqSettings);

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

    builder.Services.AddControllers();
    builder.Services.AddCommonHealthChecks(builder.Configuration);

    //register services
    builder.Services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
    builder.Services.AddScoped<ICityWeatherForecastService, CityWeatherForecastService>();
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseDefaultScalar();
    app.UseDefaultExceptionHandler();
    app.UseHttpsRedirection();
    app
        .UseRouting()
        .UseCommonHealthChecks();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
}

await app.RunWithLoggerAsync();