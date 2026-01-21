using System;
using System.Net.Http;
using CitiesGrpcService;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcCitiesClient;

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
            //o.Address = new Uri("http://citiesgrpcservice:80"); //TODO add to settings
            //o.Address = new Uri("http://localhost:8681"); //TODO add to settings
            //o.Address = new Uri("https://localhost:5031"); //TODO add to settings
            o.Address = new Uri("http://localhost:5030"); //TODO add to settings
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