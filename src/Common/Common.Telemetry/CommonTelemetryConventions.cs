namespace Common.Telemetry;

public static class CommonTelemetryConventions
{
    public static class ConfigurationKeys
    {
        /// <summary>Shared OTLP endpoint used as the fallback for configured exporters.</summary>
        public const string OtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

        /// <summary>Shared OTLP protocol used as the fallback for configured exporters.</summary>
        public const string OtlpProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";

        /// <summary>OTLP endpoint used for metrics export.</summary>
        public const string OtlpMetricsEndpoint = "OTEL_EXPORTER_OTLP_METRICS_ENDPOINT";

        /// <summary>OTLP protocol used for metrics export.</summary>
        public const string OtlpMetricsProtocol = "OTEL_EXPORTER_OTLP_METRICS_PROTOCOL";

        /// <summary>OTLP endpoint used for traces export.</summary>
        public const string OtlpTracesEndpoint = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT";

        /// <summary>OTLP protocol used for traces export.</summary>
        public const string OtlpTracesProtocol = "OTEL_EXPORTER_OTLP_TRACES_PROTOCOL";

        /// <summary>Enables the Prometheus scraping endpoint when set to true.</summary>
        public const string PrometheusEndpointEnabled = "SWS_TELEMETRY_PROMETHEUS_ENDPOINT_ENABLED";

        /// <summary>Includes health, ping, root, and metrics endpoints in traces unless set to false.</summary>
        public const string TracesIncludeInfraEndpoints = "SWS_TELEMETRY_TRACES_INCLUDE_INFRA_ENDPOINTS";
    }

    public static class OtlpProtocolValues
    {
        /// <summary>Configures OTLP export over gRPC.</summary>
        public const string Grpc = "grpc";

        /// <summary>Configures OTLP export over HTTP using protobuf payloads.</summary>
        public const string HttpProtobuf = "http/protobuf";
    }

    public static class Resources
    {
        /// <summary>Namespace applied to all Simple Weather Site telemetry resources.</summary>
        public const string ServiceNamespace = "sws";

        /// <summary>OpenTelemetry resource attribute that stores the hosting environment name.</summary>
        public const string DeploymentEnvironmentName = "deployment.environment.name";
    }

    public static class MeterNames
    {
        /// <summary>ASP.NET Core hosting metrics emitted by the runtime.</summary>
        public const string AspNetCoreHosting = "Microsoft.AspNetCore.Hosting";

        /// <summary>Kestrel server metrics emitted by ASP.NET Core.</summary>
        public const string Kestrel = "Microsoft.AspNetCore.Server.Kestrel";

        /// <summary>HTTP client metrics emitted by System.Net.Http.</summary>
        public const string HttpClient = "System.Net.Http";

        /// <summary>DNS lookup metrics emitted by System.Net.NameResolution.</summary>
        public const string NameResolution = "System.Net.NameResolution";

        /// <summary>.NET runtime metrics such as GC, threads, and assemblies.</summary>
        public const string Runtime = "System.Runtime";

        /// <summary>Entity Framework Core metrics emitted by database operations.</summary>
        public const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    }

    public static class InfraEndpointPaths
    {
        /// <summary>Application root endpoint used by simple availability checks.</summary>
        public const string Root = "/";

        /// <summary>Ping endpoint used by lightweight liveness checks.</summary>
        public const string Ping = "/ping";

        /// <summary>Prometheus scraping endpoint.</summary>
        public const string Metrics = "/metrics";

        /// <summary>Health endpoint prefix used by health checks.</summary>
        public const string Health = "/health";
    }
}
