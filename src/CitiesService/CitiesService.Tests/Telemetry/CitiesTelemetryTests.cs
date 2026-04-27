using System.Diagnostics;
using System.Diagnostics.Metrics;
using CitiesService.Application.Telemetry;

namespace CitiesService.Tests.Telemetry;

public class CitiesTelemetryTests
{
    [Fact]
    public void StartApplicationActivity_UsesCitiesServiceActivitySource()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == CitiesTelemetry.ApplicationActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = CitiesTelemetry.StartApplicationActivity("GetCitiesQuery");

        Assert.NotNull(activity);
        Assert.Equal(CitiesTelemetry.ApplicationActivitySourceName, activity!.Source.Name);
        Assert.Equal("GetCitiesQuery", activity.OperationName);
        Assert.Contains(activity.Tags, tag => tag.Key == "operation" && tag.Value == "GetCitiesQuery");
    }

    [Fact]
    public void RecordCacheRequest_PublishesLowCardinalityTags()
    {
        var measurements = new List<MetricMeasurement<long>>();
        using var listener = new MeterListener();

        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == CitiesTelemetry.ApplicationMeterName
                && instrument.Name == CitiesTelemetry.CacheRequestsMetricName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, _) =>
        {
            var tagDictionary = new Dictionary<string, object?>();
            foreach (var tag in tags)
            {
                tagDictionary[tag.Key] = tag.Value;
            }

            measurements.Add(new MetricMeasurement<long>(
                instrument.Name,
                measurement,
                tagDictionary));
        });

        listener.Start();

        CitiesTelemetry.RecordCacheRequest("TelemetryTestOperation", "hit");

        var cacheMeasurement = Assert.Single(
            measurements,
            measurement => measurement.InstrumentName == CitiesTelemetry.CacheRequestsMetricName
                           && (string?)measurement.Tags["operation"] == "TelemetryTestOperation");

        Assert.Equal(1, cacheMeasurement.Value);
        Assert.Equal("TelemetryTestOperation", cacheMeasurement.Tags["operation"]);
        Assert.Equal("hit", cacheMeasurement.Tags["cache_result"]);
        Assert.DoesNotContain("city_name", cacheMeasurement.Tags.Keys);
        Assert.DoesNotContain("cache_key", cacheMeasurement.Tags.Keys);
    }

    private sealed record MetricMeasurement<T>(
        string InstrumentName,
        T Value,
        IReadOnlyDictionary<string, object?> Tags);
}
