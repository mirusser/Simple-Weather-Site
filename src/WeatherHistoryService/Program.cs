using System.Reflection;
using Common.Application.Mapping;
using Common.Presentation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WeatherHistoryService.Listeners;
using WeatherHistoryService.Mongo;
using WeatherHistoryService.Services;
using WeatherHistoryService.Services.Contracts;
using WeatherHistoryService.Settings;
using Common.Application.HealthChecks;
using Common.Shared;

var builder = WebApplication.CreateBuilder(args);
{
    var executingAssembly = Assembly.GetExecutingAssembly();
    builder.Host.UseSerilog();

    builder.Services.AddCommonPresentationLayer(builder.Configuration);
    builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection(nameof(MongoSettings)));

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));
    builder.Services.AddMappings(executingAssembly);

    builder.Services.AddMassTransit(config =>
    {
        RabbitMQSettings rabbitMQSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

        config.AddConsumer<GotWeatherForecastListener>();
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

    builder.Services.AddControllers();
    builder.Services.AddSharedLayer(builder.Configuration);
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