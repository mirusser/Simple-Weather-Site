using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CitiesService.Application.Telemetry;

public static class CitiesTelemetry
{
    public const string ApplicationActivitySourceName = "CitiesService.Application";
    public const string ApplicationMeterName = "CitiesService.Application";

    public const string MediatorRequestDurationMetricName = "sws.cities.mediator.request.duration";
    public const string MediatorRequestsMetricName = "sws.cities.mediator.requests";
    public const string CacheRequestsMetricName = "sws.cities.cache.requests";
    public const string ReturnedCitiesMetricName = "sws.cities.returned";
    public const string SeedingRunsMetricName = "sws.cities.seeding.runs";
    public const string SeedingDurationMetricName = "sws.cities.seeding.duration";

    public static readonly ActivitySource ApplicationActivitySource = new(ApplicationActivitySourceName);

    private static readonly Meter ApplicationMeter = new(ApplicationMeterName);

    private static readonly Histogram<double> MediatorRequestDuration =
        ApplicationMeter.CreateHistogram<double>(
            MediatorRequestDurationMetricName,
            unit: "s",
            description: "Duration of CitiesService mediator requests.");

    private static readonly Counter<long> MediatorRequests =
        ApplicationMeter.CreateCounter<long>(
            MediatorRequestsMetricName,
            unit: "{request}",
            description: "Number of CitiesService mediator requests.");

    private static readonly Counter<long> CacheRequests =
        ApplicationMeter.CreateCounter<long>(
            CacheRequestsMetricName,
            unit: "{request}",
            description: "Number of CitiesService cache lookups.");

    private static readonly Histogram<long> ReturnedCities =
        ApplicationMeter.CreateHistogram<long>(
            ReturnedCitiesMetricName,
            unit: "{city}",
            description: "Number of cities returned by CitiesService operations.");

    private static readonly Counter<long> SeedingRuns =
        ApplicationMeter.CreateCounter<long>(
            SeedingRunsMetricName,
            unit: "{run}",
            description: "Number of CitiesService seeding attempts.");

    private static readonly Histogram<double> SeedingDuration =
        ApplicationMeter.CreateHistogram<double>(
            SeedingDurationMetricName,
            unit: "s",
            description: "Duration of CitiesService seeding attempts.");

    public static Activity? StartApplicationActivity(string operation)
    {
        var activity = ApplicationActivitySource.StartActivity(operation);
        activity?.SetTag("operation", operation);

        return activity;
    }

    public static void RecordMediatorRequest(
        string operation,
        TimeSpan elapsed,
        string result,
        string? errorType = null)
    {
        var tags = CreateOperationTags(operation, result, errorType);

        MediatorRequests.Add(1, tags);
        MediatorRequestDuration.Record(elapsed.TotalSeconds, tags);
    }

    public static void RecordCacheRequest(string operation, string cacheResult)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "cache_result", cacheResult }
        };

        CacheRequests.Add(1, tags);
    }

    public static void RecordReturnedCities(string operation, long count, string result)
    {
        var tags = CreateOperationTags(operation, result);
        ReturnedCities.Record(count, tags);
    }

    public static void RecordSeedingRun(
        string operation,
        TimeSpan elapsed,
        string result,
        string? errorType = null)
    {
        var tags = CreateOperationTags(operation, result, errorType);

        SeedingRuns.Add(1, tags);
        SeedingDuration.Record(elapsed.TotalSeconds, tags);
    }

    private static TagList CreateOperationTags(
        string operation,
        string result,
        string? errorType = null)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "result", result }
        };

        if (!string.IsNullOrWhiteSpace(errorType))
        {
            tags.Add("error_type", errorType);
        }

        return tags;
    }
}
