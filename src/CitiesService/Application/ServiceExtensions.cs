using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Managers;
using Application.Managers;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Docs.Swagger;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Domain.Settings;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQModels.Email;

namespace Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMQSettings = new();
                configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

                config.SetKebabCaseEndpointNameFormatter();
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMQSettings.Host);
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddMassTransitHostedService(waitUntilStarted: true);

            services.AddConvey()
                //    .AddConsul()
                .AddSwaggerDocs()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddQueryHandlers()
                .AddServiceBusEventDispatcher()
                .AddServiceBusCommandDispatcher()
                .AddInMemoryCommandDispatcher()
                // .AddInMemoryEventDispatcher()
                .AddInMemoryQueryDispatcher()
                //    .AddRedis()
                .AddRabbitMq()
                //.AddMongo()
                .Build();

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
            app.UseSwaggerDocs();
            app.UseConvey();
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
            //    Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            //}

            return app;
        }
    }
}