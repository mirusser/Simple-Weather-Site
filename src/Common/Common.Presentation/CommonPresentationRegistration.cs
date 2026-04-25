using System.Reflection;
using Common.Presentation.Exceptions;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;

namespace Common.Presentation;

public static class CommonPresentationRegistration
{
    private const string ServiceNamespace = "sws";

    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCommonPresentationLayer()
        {
            builder.Host.UseSerilog((ctx, services, cfg) =>
                cfg.ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

            builder.Services.AddSharedLayer(builder.Configuration);
            builder.Services.AddCommonOpenTelemetryMetrics(
                builder.Configuration,
                builder.Environment.ApplicationName,
                builder.Environment.EnvironmentName);
            
            return builder;
        }
    }

    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseDefaultExceptionHandler()
            => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }

    private static IServiceCollection AddCommonOpenTelemetryMetrics(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string environmentName)
    {
        if (!HasOtlpEndpoint(configuration))
        {
            return services;
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                    serviceName: applicationName,
                    serviceNamespace: ServiceNamespace,
                    serviceVersion: GetServiceVersion(),
                    serviceInstanceId: GetServiceInstanceId())
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment.name", environmentName)
                ]))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        return services;
    }

    private static bool HasOtlpEndpoint(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
           || !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"]);

    private static string GetServiceVersion()
        => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
           ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
           ?? "unknown";

    private static string GetServiceInstanceId()
        => $"{Environment.MachineName}:{Environment.ProcessId}";
}
