using Common.Presentation;
using Common.Presentation.Exceptions.Handlers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SignalRServer.Hubs.Site;
using SignalRServer.Listeners;
using SignalRServer.Services;
using SignalRServer.Services.Contracts;
using SignalRServer.Settings;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();

    builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection(nameof(MongoSettings)));
    builder.Services.Configure<HubMethods>(builder.Configuration.GetSection(nameof(HubMethods)));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed((_) => true)
                .AllowCredentials();
            });
    });

    builder.Services.AddSignalR();

    builder.Services.AddMassTransit(config =>
    {
        RabbitMQSettings rabbitMQSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

        config.AddConsumer<CreatedCityWeatherForecastSearchListener>();
        config.SetKebabCaseEndpointNameFormatter();

        config.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitMQSettings.Host);
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
}

var app = builder.Build();
{
    app.UseServiceExceptionHandler();

    app.UseCors("AllowAll");

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHub<WeatherHistoryHub>($"/{nameof(WeatherHistoryHub)}");
    });

    //app.UseSignalR(routes =>
    //{
    //    routes.MapHub<TestHub>("/test");
    //});

    WebApplicationStartup.Run(app);
}