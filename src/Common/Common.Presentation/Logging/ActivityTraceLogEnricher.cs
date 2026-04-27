using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Common.Presentation.Logging;

public sealed class ActivityTraceLogEnricher : ILogEventEnricher
{
    public const string EnabledConfigurationKey = "SWS_TELEMETRY_LOG_TRACE_CORRELATION_ENABLED";

    public static bool IsEnabled(IConfiguration configuration)
        => bool.TryParse(configuration[EnabledConfigurationKey], out var enabled) && enabled;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace_id", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span_id", activity.SpanId.ToString()));
    }
}
