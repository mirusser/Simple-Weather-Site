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
using Common.Shared;

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

    builder.Services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
    builder.Services.AddCommonHealthChecks(builder.Configuration);
}

var app = builder.Build();
{
    app.UseDefaultScalar();
    app.UseDefaultExceptionHandler();

    app.UseCors("AllowAll");

    app.UseHttpsRedirection();
    app
        .UseRouting()
        .UseCommonHealthChecks();

    app.MapHub<WeatherHistoryHub>($"/{nameof(WeatherHistoryHub)}");

    //app.UseSignalR(routes =>
    //{
    //    routes.MapHub<TestHub>("/test");
    //});
}

await app.RunWithLoggerAsync();