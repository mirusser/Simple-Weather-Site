using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Common.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(JsonSerializerOptions.Default);

        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseDefaultScalar(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "API Reference";
            options.Telemetry = false;
        });

        return app;
    }

    /// <summary>
    /// Page with basic environment information about the service
    /// To ease getting environment config data
    /// </summary>
    public static IEndpointRouteBuilder UseServiceStartupPage(this IEndpointRouteBuilder app,
        IHostEnvironment environment)
    {
        app.MapGet("/", () => environment);

        return app;
    }
}