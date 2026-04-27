using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using CitiesService.Application.Telemetry;

namespace CitiesGrpcService.Telemetry;

public static class CitiesGrpcTelemetry
{
    public const string ActivitySourceName = CitiesTelemetryConventions.ActivitySources.Grpc;
    public const string MeterName = CitiesTelemetryConventions.Meters.Grpc;

    public const string CallsMetricName = CitiesTelemetryConventions.MetricNames.GrpcCalls;
    public const string CallDurationMetricName = CitiesTelemetryConventions.MetricNames.GrpcCallDuration;
    public const string StreamMessagesMetricName = CitiesTelemetryConventions.MetricNames.GrpcStreamMessages;

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> Calls =
        Meter.CreateCounter<long>(
            CallsMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Call,
            description: CitiesTelemetryConventions.MetricDescriptions.GrpcCalls);

    private static readonly Histogram<double> CallDuration =
        Meter.CreateHistogram<double>(
            CallDurationMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Seconds,
            description: CitiesTelemetryConventions.MetricDescriptions.GrpcCallDuration);

    private static readonly Counter<long> StreamMessages =
        Meter.CreateCounter<long>(
            StreamMessagesMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Message,
            description: CitiesTelemetryConventions.MetricDescriptions.GrpcStreamMessages);

    public static Activity? StartActivity(string method, string grpcType)
    {
        var activity = ActivitySource.StartActivity(method);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.GrpcMethod, method);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.GrpcType, grpcType);

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
            { CitiesTelemetryConventions.TagNames.GrpcMethod, method }
        };

        StreamMessages.Add(1, tags);
    }

    public static void SetResult(Activity? activity, string result)
    {
        activity?.SetStatus(result == CitiesTelemetryConventions.ResultValues.Success
            ? ActivityStatusCode.Ok
            : ActivityStatusCode.Error);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.Result, result);
    }

    public static void SetException(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.Result, CitiesTelemetryConventions.ResultValues.Exception);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.ErrorType, exception.GetType().Name);
    }

    private static TagList CreateTags(
        string method,
        string grpcType,
        string result,
        string? errorType = null)
    {
        var tags = new TagList
        {
            { CitiesTelemetryConventions.TagNames.GrpcMethod, method },
            { CitiesTelemetryConventions.TagNames.GrpcType, grpcType },
            { CitiesTelemetryConventions.TagNames.Result, result }
        };

        if (!string.IsNullOrWhiteSpace(errorType))
        {
            tags.Add(CitiesTelemetryConventions.TagNames.ErrorType, errorType);
        }

        return tags;
    }
}
