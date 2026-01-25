using System;
using System.Reflection;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Contracts.HealthCheck;
using Common.Infrastructure;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WeatherService.Clients;
using WeatherService.HealthCheck;
using WeatherService.Settings;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddCommonInfrastructure(builder.Configuration);
    builder.AddCommonPresentationLayer();

    builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection(nameof(ServiceSettings)));

    builder.Services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        RabbitMqSettings rabbitMqSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMqSettings)).Bind(rabbitMqSettings);

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitMqSettings.Host);

            cfg.ConfigureEndpoints(ctx);
        });
    });

    builder.Services
        .AddOptions<MassTransitHostOptions>()
        .Configure(options =>
        {
            // if specified, waits until the bus is started before
            // returning from IHostedService.StartAsync
            // default is false
            options.WaitUntilStarted = true;

            // if specified, limits the wait time when starting the bus
            options.StartTimeout = TimeSpan.FromSeconds(10);

            // if specified, limits the wait time when stopping the bus
            options.StopTimeout = TimeSpan.FromSeconds(30);
        });

    var executingAssembly = Assembly.GetExecutingAssembly();
    builder.Services.AddMappings(executingAssembly);
    builder.Services.AddValidatorsFromAssembly(executingAssembly);

    builder.Services.AddHttpClient("OpenWeather", (sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ServiceSettings>>().Value;
        client.BaseAddress = new Uri(settings.OpenWeatherHost);
    });

    builder.Services.AddTransient<WeatherClient>();

    builder.Services.AddControllers();

    builder.Services.AddCommonHealthChecks()
        .AddCheck<OpenWeatherExternalEndpointHealthCheck>(
            name: "OpenWeather",
            failureStatus: HealthStatus.Degraded,
            tags: [nameof(HealthChecksTags.Database)]);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseDefaultExceptionHandler();

    app.UseDefaultScalar();

    //app.UseHttpsRedirection();
    app
        .UseRouting()
        .UseCommonHealthChecks();
    app.UseAuthorization();

    app.MapControllers();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();