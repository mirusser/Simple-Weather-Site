using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CitiesService.Application.Telemetry;

public static class CitiesTelemetry
{
    public const string ApplicationActivitySourceName = CitiesTelemetryConventions.ActivitySources.Application;
    public const string ApplicationMeterName = CitiesTelemetryConventions.Meters.Application;

    public const string MediatorRequestDurationMetricName = CitiesTelemetryConventions.MetricNames.MediatorRequestDuration;
    public const string MediatorRequestsMetricName = CitiesTelemetryConventions.MetricNames.MediatorRequests;
    public const string CacheRequestsMetricName = CitiesTelemetryConventions.MetricNames.CacheRequests;
    public const string ReturnedCitiesMetricName = CitiesTelemetryConventions.MetricNames.ReturnedCities;
    public const string SeedingRunsMetricName = CitiesTelemetryConventions.MetricNames.SeedingRuns;
    public const string SeedingDurationMetricName = CitiesTelemetryConventions.MetricNames.SeedingDuration;

    public static readonly ActivitySource ApplicationActivitySource = new(ApplicationActivitySourceName);

    private static readonly Meter ApplicationMeter = new(ApplicationMeterName);

    private static readonly Histogram<double> MediatorRequestDuration =
        ApplicationMeter.CreateHistogram<double>(
            MediatorRequestDurationMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Seconds,
            description: CitiesTelemetryConventions.MetricDescriptions.MediatorRequestDuration);

    private static readonly Counter<long> MediatorRequests =
        ApplicationMeter.CreateCounter<long>(
            MediatorRequestsMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Request,
            description: CitiesTelemetryConventions.MetricDescriptions.MediatorRequests);

    private static readonly Counter<long> CacheRequests =
        ApplicationMeter.CreateCounter<long>(
            CacheRequestsMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Request,
            description: CitiesTelemetryConventions.MetricDescriptions.CacheRequests);

    private static readonly Histogram<long> ReturnedCities =
        ApplicationMeter.CreateHistogram<long>(
            ReturnedCitiesMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.City,
            description: CitiesTelemetryConventions.MetricDescriptions.ReturnedCities);

    private static readonly Counter<long> SeedingRuns =
        ApplicationMeter.CreateCounter<long>(
            SeedingRunsMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Run,
            description: CitiesTelemetryConventions.MetricDescriptions.SeedingRuns);

    private static readonly Histogram<double> SeedingDuration =
        ApplicationMeter.CreateHistogram<double>(
            SeedingDurationMetricName,
            unit: CitiesTelemetryConventions.MetricUnits.Seconds,
            description: CitiesTelemetryConventions.MetricDescriptions.SeedingDuration);

    public static Activity? StartApplicationActivity(string operation)
    {
        var activity = ApplicationActivitySource.StartActivity(operation);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.Operation, operation);

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
            { CitiesTelemetryConventions.TagNames.Operation, operation },
            { CitiesTelemetryConventions.TagNames.CacheResult, cacheResult }
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
            { CitiesTelemetryConventions.TagNames.Operation, operation },
            { CitiesTelemetryConventions.TagNames.Result, result }
        };

        if (!string.IsNullOrWhiteSpace(errorType))
        {
            tags.Add(CitiesTelemetryConventions.TagNames.ErrorType, errorType);
        }

        return tags;
    }
}
