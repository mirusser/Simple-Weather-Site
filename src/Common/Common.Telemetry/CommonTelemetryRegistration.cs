using System.Reflection;
using Common.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Common.Telemetry;

public static class CommonTelemetryRegistration
{
    private const string ServiceNamespace = "sws";

    public static IServiceCollection AddCommonTelemetry(
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