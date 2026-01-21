using System;
using System.Net.Http;
using CitiesGrpcService;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using GrpcCitiesClient.Settings;
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
        
        GrpcCitiesClientConnections citiesClientConnections = new();
        configuration.GetSection(nameof(GrpcCitiesClientConnections)).Bind(citiesClientConnections);

        services.AddGrpcClient<Cities.CitiesClient>("Cities", o =>
        {
            o.Address = new Uri(citiesClientConnections.Uri);
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