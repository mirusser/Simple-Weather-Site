using System;
using System.Reflection;
using CitiesService.Domain.Settings;
using Common.Application.Mapping;
using Common.Mediator.DependencyInjection;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.Application;

public static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationLayer(IConfiguration configuration)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
        
            services.AddValidatorsFromAssembly(executingAssembly);
        
            // TODO: do we really need all assemblies here?
            services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());

            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMqSettings = new();
                configuration
                    .GetSection(nameof(RabbitMQSettings))
                    .Bind(rabbitMqSettings);

                // TODO: after adding a job uncomment the lines
                //config.AddConsumer<JobListener>();

                config.SetKebabCaseEndpointNameFormatter();
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Host);
                    cfg.ConfigureEndpoints(ctx);
                });
            });

            services.AddOptions<MassTransitHostOptions>()
                .Configure(options =>
                {
                    // if specified, waits until the bus is started before
                    // returning from IHostedService.StartAsync
                    // default is false
                    options.WaitUntilStarted = false;

                    // if specified, limits the wait time when starting the bus
                    //options.StartTimeout = TimeSpan.FromSeconds(10);

                    // if specified, limits the wait time when stopping the bus
                    //options.StopTimeout = TimeSpan.FromSeconds(30);
                });

            services.AddMappings(executingAssembly);

            return services;
        }
    }
}