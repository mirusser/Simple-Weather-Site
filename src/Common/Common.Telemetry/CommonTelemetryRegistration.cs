using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Telemetry;

public sealed class CommonTelemetryOptions
{
    /// <summary>Additional application meter names to include in the metrics pipeline.</summary>
    public IReadOnlyCollection<string> MeterNames { get; init; } = [];

    /// <summary>Additional activity source names to include in the tracing pipeline.</summary>
    public IReadOnlyCollection<string> ActivitySourceNames { get; init; } = [];
}

public static class CommonTelemetryRegistration
{
    private static readonly string[] BuiltInMeterNames =
    [
        CommonTelemetryConventions.MeterNames.AspNetCoreHosting,
        CommonTelemetryConventions.MeterNames.Kestrel,
        CommonTelemetryConventions.MeterNames.HttpClient,
        CommonTelemetryConventions.MeterNames.NameResolution,
        CommonTelemetryConventions.MeterNames.Runtime,
        CommonTelemetryConventions.MeterNames.EntityFrameworkCore
    ];

    public static IServiceCollection AddCommonTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string environmentName,
        CommonTelemetryOptions? options = null)
    {
        options ??= new CommonTelemetryOptions();
        services.Configure<CommonTelemetryConfigurationOptions>(configuration);

        var telemetryConfiguration = ReadTelemetryConfiguration(configuration);
        var hasOtlpMetricsEndpoint = HasOtlpMetricsEndpoint(telemetryConfiguration);
        var hasPrometheusEndpoint = telemetryConfiguration.IsPrometheusEndpointEnabled;
        var hasTracesEndpoint = HasTracesEndpoint(telemetryConfiguration);

        if (!hasOtlpMetricsEndpoint && !hasPrometheusEndpoint && !hasTracesEndpoint)
        {
            return services;
        }

        var telemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                    serviceName: applicationName,
                    serviceNamespace: CommonTelemetryConventions.Resources.ServiceNamespace,
                    serviceVersion: GetServiceVersion(),
                    serviceInstanceId: GetServiceInstanceId())
                .AddAttributes([
                    new KeyValuePair<string, object>(
                        CommonTelemetryConventions.Resources.DeploymentEnvironmentName,
                        environmentName)
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
                    metrics.AddOtlpExporter(exporter =>
                        ConfigureOtlpExporter(
                            exporter,
                            telemetryConfiguration,
                            telemetryConfiguration.OtlpMetricsEndpoint,
                            telemetryConfiguration.OtlpMetricsProtocol));
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
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        if (!telemetryConfiguration.ShouldIncludeInfraEndpointTraces)
                        {
                            options.Filter = context => !IsInfraEndpoint(context.Request.Path);
                        }
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (options.ActivitySourceNames.Count > 0)
                {
                    tracing.AddSource(options.ActivitySourceNames.ToArray());
                }

                tracing.AddOtlpExporter(exporter =>
                    ConfigureOtlpExporter(
                        exporter,
                        telemetryConfiguration,
                        telemetryConfiguration.OtlpTracesEndpoint,
                        telemetryConfiguration.OtlpTracesProtocol));
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
        => ReadTelemetryConfiguration(configuration).IsPrometheusEndpointEnabled;

    public static bool ShouldIncludeInfraEndpointTraces(IConfiguration configuration)
        => ReadTelemetryConfiguration(configuration).ShouldIncludeInfraEndpointTraces;

    private static CommonTelemetryConfigurationOptions ReadTelemetryConfiguration(IConfiguration configuration)
        => configuration.Get<CommonTelemetryConfigurationOptions>() ?? new CommonTelemetryConfigurationOptions();

    private static bool HasOtlpMetricsEndpoint(CommonTelemetryConfigurationOptions options)
        => !string.IsNullOrWhiteSpace(options.OtlpEndpoint)
           || !string.IsNullOrWhiteSpace(options.OtlpMetricsEndpoint);

    private static bool HasTracesEndpoint(CommonTelemetryConfigurationOptions options)
        => !string.IsNullOrWhiteSpace(options.OtlpTracesEndpoint);

    private static void ConfigureOtlpExporter(
        OtlpExporterOptions exporterOptions,
        CommonTelemetryConfigurationOptions telemetryOptions,
        string? signalEndpoint,
        string? signalProtocol)
    {
        var endpoint =
            signalEndpoint
            ?? telemetryOptions.OtlpEndpoint;
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            exporterOptions.Endpoint = new Uri(endpoint);
        }

        var protocol =
            signalProtocol
            ?? telemetryOptions.OtlpProtocol;
        if (string.Equals(
                protocol,
                CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf,
                StringComparison.OrdinalIgnoreCase))
        {
            exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
        }
        else if (string.Equals(
                     protocol,
                     CommonTelemetryConventions.OtlpProtocolValues.Grpc,
                     StringComparison.OrdinalIgnoreCase))
        {
            exporterOptions.Protocol = OtlpExportProtocol.Grpc;
        }
    }

    private static string GetServiceVersion()
        => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
           ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
           ?? "unknown";

    private static string GetServiceInstanceId()
        => $"{Environment.MachineName}:{Environment.ProcessId}";

    private static bool IsInfraEndpoint(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (string.Equals(value, CommonTelemetryConventions.InfraEndpointPaths.Root, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, CommonTelemetryConventions.InfraEndpointPaths.Ping, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, CommonTelemetryConventions.InfraEndpointPaths.Metrics, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return value.StartsWith(CommonTelemetryConventions.InfraEndpointPaths.Health, StringComparison.OrdinalIgnoreCase)
               && (value.Length == CommonTelemetryConventions.InfraEndpointPaths.Health.Length
                   || value[CommonTelemetryConventions.InfraEndpointPaths.Health.Length] == '/');
    }
}
