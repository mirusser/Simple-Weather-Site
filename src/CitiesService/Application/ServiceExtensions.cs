using System.Reflection;
using Application.Interfaces.Managers;
using Application.Managers;
using Application.PipelineBehaviours;
using Domain.Settings;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

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
            services.AddMassTransitHostedService(waitUntilStarted: true);

            #region Managers

            services.AddTransient<ICityManager, CityManager>();

            #endregion Managers
        }

        public static void AddApplicationLayerAutomapper(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        public static IApplicationBuilder UseApplicationLayer(this IApplicationBuilder app)
        {
            return app;
        }
    }
}