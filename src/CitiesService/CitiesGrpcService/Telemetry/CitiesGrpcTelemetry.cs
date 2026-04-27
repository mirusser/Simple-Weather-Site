using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CitiesGrpcService.Telemetry;

public static class CitiesGrpcTelemetry
{
    public const string ActivitySourceName = "CitiesGrpcService";
    public const string MeterName = "CitiesGrpcService";

    public const string CallsMetricName = "sws.cities.grpc.calls";
    public const string CallDurationMetricName = "sws.cities.grpc.call.duration";
    public const string StreamMessagesMetricName = "sws.cities.grpc.stream.messages";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> Calls =
        Meter.CreateCounter<long>(
            CallsMetricName,
            unit: "{call}",
            description: "Number of Cities gRPC calls.");

    private static readonly Histogram<double> CallDuration =
        Meter.CreateHistogram<double>(
            CallDurationMetricName,
            unit: "s",
            description: "Duration of Cities gRPC calls.");

    private static readonly Counter<long> StreamMessages =
        Meter.CreateCounter<long>(
            StreamMessagesMetricName,
            unit: "{message}",
            description: "Number of Cities gRPC stream messages sent.");

    public static Activity? StartActivity(string method, string grpcType)
    {
        var activity = ActivitySource.StartActivity(method);
        activity?.SetTag("grpc_method", method);
        activity?.SetTag("grpc_type", grpcType);

        return activity;
    }

    public static void RecordCall(
        string method,
        string grpcType,
        TimeSpan elapsed,
        string result,
        string? errorType = null)
    {
        var tags = CreateTags(method, grpcType, result, errorType);

        Calls.Add(1, tags);
        CallDuration.Record(elapsed.TotalSeconds, tags);
    }

    public static void RecordStreamMessage(string method)
    {
        var tags = new TagList
        {
            { "grpc_method", method }
        };

        StreamMessages.Add(1, tags);
    }

    public static void SetResult(Activity? activity, string result)
    {
        activity?.SetStatus(result == "success" ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity?.SetTag("result", result);
    }

    public static void SetException(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        activity?.SetTag("result", "exception");
        activity?.SetTag("error_type", exception.GetType().Name);
    }

    private static TagList CreateTags(
        string method,
        string grpcType,
        string result,
        string? errorType = null)
    {
        var tags = new TagList
        {
            { "grpc_method", method },
            { "grpc_type", grpcType },
            { "result", result }
        };

        if (!string.IsNullOrWhiteSpace(errorType))
        {
            tags.Add("error_type", errorType);
        }

        return tags;
    }
}
