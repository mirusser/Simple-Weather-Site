using System;
using System.Reflection;
using Common.Presentation;
using Common.Presentation.Exceptions.Handlers;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using WeatherService.Clients;
using WeatherService.HealthCheck;
using WeatherService.Settings;
using Common.Application.Mapping;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();

    builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection(nameof(ServiceSettings)));

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    //builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
    builder.Services.AddMappings();

    builder.Services.AddMassTransit(config =>
    {
        RabbitMQSettings rabbitMQSettings = new();
        builder.Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

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
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherService", Version = "v1" });
    });

    builder.Services.AddHttpClient<WeatherClient>()
         .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
         .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));

    builder.Services.AddHealthChecks()
        .AddCheck<ExternalEndpointHealthCheck>("OpenWeather");
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseServiceExceptionHandler();

    #region Swagger

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherService v1"));

    #endregion Swagger

    //app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/api/weatherforecast/health");
    });

    WebApplicationStartup.Run(app);
}