using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Telemetry.Tests;

public class CommonTelemetryConfigurationOptionsTests
{
    [Fact]
    public void CommonTelemetryConfigurationOptions_BindsEnvironmentStyleKeys()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [CommonTelemetryConventions.ConfigurationKeys.OtlpEndpoint] = "http://collector:4317",
            [CommonTelemetryConventions.ConfigurationKeys.OtlpProtocol] = CommonTelemetryConventions.OtlpProtocolValues.Grpc,
            [CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsEndpoint] = "http://collector:4318/v1/metrics",
            [CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsProtocol] = CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf,
            [CommonTelemetryConventions.ConfigurationKeys.OtlpTracesEndpoint] = "http://collector:4318/v1/traces",
            [CommonTelemetryConventions.ConfigurationKeys.OtlpTracesProtocol] = CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf,
            [CommonTelemetryConventions.ConfigurationKeys.PrometheusEndpointEnabled] = "true",
            [CommonTelemetryConventions.ConfigurationKeys.TracesIncludeInfraEndpoints] = "false"
        });

        var options = configuration.Get<CommonTelemetryConfigurationOptions>();

        Assert.NotNull(options);
        Assert.Equal("http://collector:4317", options.OtlpEndpoint);
        Assert.Equal(CommonTelemetryConventions.OtlpProtocolValues.Grpc, options.OtlpProtocol);
        Assert.Equal("http://collector:4318/v1/metrics", options.OtlpMetricsEndpoint);
        Assert.Equal(CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf, options.OtlpMetricsProtocol);
        Assert.Equal("http://collector:4318/v1/traces", options.OtlpTracesEndpoint);
        Assert.Equal(CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf, options.OtlpTracesProtocol);
        Assert.Equal("true", options.PrometheusEndpointEnabled);
        Assert.Equal("false", options.TracesIncludeInfraEndpoints);
    }

    [Fact]
    public void AddCommonTelemetry_RegistersBoundConfigurationOptions()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [CommonTelemetryConventions.ConfigurationKeys.TracesIncludeInfraEndpoints] = "false"
        });
        var services = new ServiceCollection();

        services.AddCommonTelemetry(configuration, "TestApp", "Test");
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<CommonTelemetryConfigurationOptions>>().Value;
        Assert.Equal("false", options.TracesIncludeInfraEndpoints);
        Assert.False(options.ShouldIncludeInfraEndpointTraces);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("not-bool", false)]
    public void IsPrometheusEndpointEnabled_MatchesBooleanTryParseBehavior(
        string? value,
        bool expected)
    {
        var configuration = BuildConfiguration(
            CommonTelemetryConventions.ConfigurationKeys.PrometheusEndpointEnabled,
            value);

        Assert.Equal(expected, CommonTelemetryRegistration.IsPrometheusEndpointEnabled(configuration));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("true", true)]
    [InlineData("not-bool", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void ShouldIncludeInfraEndpointTraces_DefaultsToIncludedAndOnlyDisablesOnFalse(
        string? value,
        bool expected)
    {
        var configuration = BuildConfiguration(
            CommonTelemetryConventions.ConfigurationKeys.TracesIncludeInfraEndpoints,
            value);

        Assert.Equal(expected, CommonTelemetryRegistration.ShouldIncludeInfraEndpointTraces(configuration));
    }

    [Fact]
    public void CommonTelemetryConventions_PreserveConfigurationAndProtocolValues()
    {
        Assert.Equal("OTEL_EXPORTER_OTLP_ENDPOINT", CommonTelemetryConventions.ConfigurationKeys.OtlpEndpoint);
        Assert.Equal("OTEL_EXPORTER_OTLP_PROTOCOL", CommonTelemetryConventions.ConfigurationKeys.OtlpProtocol);
        Assert.Equal("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT", CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsEndpoint);
        Assert.Equal("OTEL_EXPORTER_OTLP_METRICS_PROTOCOL", CommonTelemetryConventions.ConfigurationKeys.OtlpMetricsProtocol);
        Assert.Equal("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", CommonTelemetryConventions.ConfigurationKeys.OtlpTracesEndpoint);
        Assert.Equal("OTEL_EXPORTER_OTLP_TRACES_PROTOCOL", CommonTelemetryConventions.ConfigurationKeys.OtlpTracesProtocol);
        Assert.Equal("SWS_TELEMETRY_PROMETHEUS_ENDPOINT_ENABLED", CommonTelemetryConventions.ConfigurationKeys.PrometheusEndpointEnabled);
        Assert.Equal("SWS_TELEMETRY_TRACES_INCLUDE_INFRA_ENDPOINTS", CommonTelemetryConventions.ConfigurationKeys.TracesIncludeInfraEndpoints);
        Assert.Equal("grpc", CommonTelemetryConventions.OtlpProtocolValues.Grpc);
        Assert.Equal("http/protobuf", CommonTelemetryConventions.OtlpProtocolValues.HttpProtobuf);
    }

    private static IConfiguration BuildConfiguration(string key, string? value)
    {
        var values = new Dictionary<string, string?>();
        if (value is not null)
        {
            values[key] = value;
        }

        return BuildConfiguration(values);
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
