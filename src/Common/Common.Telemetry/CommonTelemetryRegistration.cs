using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Telemetry;

public sealed class CommonTelemetryOptions
{
    public IReadOnlyCollection<string> MeterNames { get; init; } = [];
    public IReadOnlyCollection<string> ActivitySourceNames { get; init; } = [];
}

public static class CommonTelemetryRegistration
{
    private const string ServiceNamespace = "sws";
    private const string PrometheusEndpointEnabledKey = "SWS_TELEMETRY_PROMETHEUS_ENDPOINT_ENABLED";

    private static readonly string[] BuiltInMeterNames =
    [
        "Microsoft.AspNetCore.Hosting",
        "Microsoft.AspNetCore.Server.Kestrel",
        "System.Net.Http",
        "System.Net.NameResolution",
        "System.Runtime",
        "Microsoft.EntityFrameworkCore"
    ];

    public static IServiceCollection AddCommonTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string environmentName,
        CommonTelemetryOptions? options = null)
    {
        options ??= new CommonTelemetryOptions();

        var hasOtlpMetricsEndpoint = HasOtlpMetricsEndpoint(configuration);
        var hasPrometheusEndpoint = IsPrometheusEndpointEnabled(configuration);
        var hasTracesEndpoint = HasTracesEndpoint(configuration);

        if (!hasOtlpMetricsEndpoint && !hasPrometheusEndpoint && !hasTracesEndpoint)
        {
            return services;
        }

        var telemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                    serviceName: applicationName,
                    serviceNamespace: ServiceNamespace,
                    serviceVersion: GetServiceVersion(),
                    serviceInstanceId: GetServiceInstanceId())
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment.name", environmentName)
                ]));

        if (hasOtlpMetricsEndpoint || hasPrometheusEndpoint)
        {
            telemetry.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(BuiltInMeterNames);

                if (options.MeterNames.Count > 0)
                {
                    metrics.AddMeter(options.MeterNames.ToArray());
                }

                if (hasOtlpMetricsEndpoint)
                {
                    metrics.AddOtlpExporter();
                }

                if (hasPrometheusEndpoint)
                {
                    metrics.AddPrometheusExporter();
                }
            });
        }

        if (hasTracesEndpoint)
        {
            telemetry.WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (options.ActivitySourceNames.Count > 0)
                {
                    tracing.AddSource(options.ActivitySourceNames.ToArray());
                }

                tracing.AddOtlpExporter();
            });
        }

        return services;
    }

    public static IApplicationBuilder UseCommonPrometheusMetrics(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        if (IsPrometheusEndpointEnabled(configuration))
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }

        return app;
    }

    public static bool IsPrometheusEndpointEnabled(IConfiguration configuration)
        => bool.TryParse(configuration[PrometheusEndpointEnabledKey], out var enabled) && enabled;

    private static bool HasOtlpMetricsEndpoint(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
           || !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"]);

    private static bool HasTracesEndpoint(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"]);

    private static string GetServiceVersion()
        => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
           ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
           ?? "unknown";

    private static string GetServiceInstanceId()
        => $"{Environment.MachineName}:{Environment.ProcessId}";
}
