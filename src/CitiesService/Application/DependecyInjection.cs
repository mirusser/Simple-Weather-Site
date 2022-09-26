using System;
using System.Reflection;
using CitiesService.Application.Interfaces.Managers;
using CitiesService.Application.Managers;
using CitiesService.Domain.Settings;
using Common.Application.Behaviors;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.Application;

public static class DependecyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddMassTransit(config =>
        {
            RabbitMQSettings rabbitMQSettings = new();
            configuration
                .GetSection(nameof(RabbitMQSettings))
                .Bind(rabbitMQSettings);

            config.SetKebabCaseEndpointNameFormatter();
            config.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMQSettings.Host);
                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddOptions<MassTransitHostOptions>()
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

        #region Managers

        services.AddTransient<ICityManager, CityManager>();

        #endregion Managers

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IApplicationBuilder UseApplicationLayer(this IApplicationBuilder app)
    {
        return app;
    }
}