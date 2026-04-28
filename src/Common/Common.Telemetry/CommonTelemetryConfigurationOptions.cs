using Microsoft.Extensions.Configuration;

namespace Common.Telemetry;

public sealed class CommonTelemetryConfigurationOptions
{
    /// <summary>Shared OTLP endpoint used as the fallback for configured exporters.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpEndpoint)]
    public string? OtlpEndpoint { get; set; }

    /// <summary>Shared OTLP protocol used as the fallback for configured exporters.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpProtocol)]
    public string? OtlpProtocol { get; set; }

    /// <summary>Signal-specific OTLP endpoint used for metrics export.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsEndpoint)]
    public string? OtlpMetricsEndpoint { get; set; }

    /// <summary>Signal-specific OTLP protocol used for metrics export.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsProtocol)]
    public string? OtlpMetricsProtocol { get; set; }

    /// <summary>Signal-specific OTLP endpoint used for traces export.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpTracesEndpoint)]
    public string? OtlpTracesEndpoint { get; set; }

    /// <summary>Signal-specific OTLP protocol used for traces export.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.OtlpTracesProtocol)]
    public string? OtlpTracesProtocol { get; set; }

    /// <summary>Raw switch value that enables the Prometheus scraping endpoint when it parses as true.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.PrometheusEndpointEnabled)]
    public string? PrometheusEndpointEnabled { get; set; }

    /// <summary>Raw switch value that excludes infrastructure endpoints from traces only when it parses as false.</summary>
    [ConfigurationKeyName(CommonTelemetryConventions.ConfigurationKeys.TracesIncludeInfraEndpoints)]
    public string? TracesIncludeInfraEndpoints { get; set; }

    /// <summary>True when Prometheus scraping should be exposed by the application.</summary>
    public bool IsPrometheusEndpointEnabled
        => bool.TryParse(PrometheusEndpointEnabled, out var enabled) && enabled;

    /// <summary>True when infrastructure endpoints should be included in ASP.NET Core traces.</summary>
    public bool ShouldIncludeInfraEndpointTraces
        => !bool.TryParse(TracesIncludeInfraEndpoints, out var include) || include;
}
