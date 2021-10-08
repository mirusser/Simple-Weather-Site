using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CitiesGrpcService;
using System.Net.Http;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Core;

namespace GrpcCitiesClient
{
    public static class ServiceExtensions
    {
        public static void AddGrpcCitiesClient(this IServiceCollection services, IConfiguration configuration)
        {
            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            services.AddGrpcClient<Cities.CitiesClient>("Cities", o =>
            {
                o.Address = new Uri("http://citiesgrpcservice:80"); //TODO add to settings
                //o.Address = new Uri("http://localhost:8681"); //TODO add to settings
            })
            .ConfigureChannel(o =>
            {
                //o.cre
                o.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
            })
            //.EnableCallContextPropagation(o => o.SuppressContextNotFoundErrors = true)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                return handler;
            });

            services.AddScoped<ICitiesClient, CitiesClient>();
        }
    }
}