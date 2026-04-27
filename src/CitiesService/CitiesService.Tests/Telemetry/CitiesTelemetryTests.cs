using System.Diagnostics;
using System.Diagnostics.Metrics;
using CitiesService.Application.Telemetry;

namespace CitiesService.Tests.Telemetry;

public class CitiesTelemetryTests
{
    [Fact]
    public void CitiesTelemetryConventions_PreserveDashboardSchemaNames()
    {
        Assert.Equal("CitiesService.Application", CitiesTelemetryConventions.ActivitySources.Application);
        Assert.Equal("CitiesService.GraphQL", CitiesTelemetryConventions.ActivitySources.GraphQl);
        Assert.Equal("CitiesGrpcService", CitiesTelemetryConventions.ActivitySources.Grpc);

        Assert.Equal("sws.cities.cache.requests", CitiesTelemetryConventions.MetricNames.CacheRequests);
        Assert.Equal("sws.cities.grpc.calls", CitiesTelemetryConventions.MetricNames.GrpcCalls);

        Assert.Equal("operation", CitiesTelemetryConventions.TagNames.Operation);
        Assert.Equal("result", CitiesTelemetryConventions.TagNames.Result);
        Assert.Equal("error_type", CitiesTelemetryConventions.TagNames.ErrorType);
        Assert.Equal("cache_result", CitiesTelemetryConventions.TagNames.CacheResult);
        Assert.Equal("grpc_method", CitiesTelemetryConventions.TagNames.GrpcMethod);
        Assert.Equal("grpc_type", CitiesTelemetryConventions.TagNames.GrpcType);

        Assert.Equal("success", CitiesTelemetryConventions.ResultValues.Success);
        Assert.Equal("exception", CitiesTelemetryConventions.ResultValues.Exception);
        Assert.Equal("hit", CitiesTelemetryConventions.CacheResults.Hit);
        Assert.Equal("cities.Cities/GetCitiesPagination", CitiesTelemetryConventions.Operations.Grpc.GetCitiesPagination);
    }

    [Fact]
    public void StartApplicationActivity_UsesCitiesServiceActivitySource()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == CitiesTelemetryConventions.ActivitySources.Application,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = CitiesTelemetry.StartApplicationActivity(
            CitiesTelemetryConventions.Operations.GetCitiesByName);

        Assert.NotNull(activity);
        Assert.Equal(CitiesTelemetryConventions.ActivitySources.Application, activity!.Source.Name);
        Assert.Equal(CitiesTelemetryConventions.Operations.GetCitiesByName, activity.OperationName);
        Assert.Contains(
            activity.Tags,
            tag => tag.Key == CitiesTelemetryConventions.TagNames.Operation
                   && tag.Value == CitiesTelemetryConventions.Operations.GetCitiesByName);
    }

    [Fact]
    public void RecordCacheRequest_PublishesLowCardinalityTags()
    {
        var measurements = new List<MetricMeasurement<long>>();
        using var listener = new MeterListener();

        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == CitiesTelemetryConventions.Meters.Application
                && instrument.Name == CitiesTelemetryConventions.MetricNames.CacheRequests)
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

        CitiesTelemetry.RecordCacheRequest(
            CitiesTelemetryConventions.Operations.GetCitiesByName,
            CitiesTelemetryConventions.CacheResults.Hit);

        var cacheMeasurement = Assert.Single(
            measurements,
            measurement => measurement.InstrumentName == CitiesTelemetryConventions.MetricNames.CacheRequests
                           && (string?)measurement.Tags[CitiesTelemetryConventions.TagNames.Operation]
                           == CitiesTelemetryConventions.Operations.GetCitiesByName);

        Assert.Equal(1, cacheMeasurement.Value);
        Assert.Equal(
            CitiesTelemetryConventions.Operations.GetCitiesByName,
            cacheMeasurement.Tags[CitiesTelemetryConventions.TagNames.Operation]);
        Assert.Equal(
            CitiesTelemetryConventions.CacheResults.Hit,
            cacheMeasurement.Tags[CitiesTelemetryConventions.TagNames.CacheResult]);
        Assert.DoesNotContain("city_name", cacheMeasurement.Tags.Keys);
        Assert.DoesNotContain("cache_key", cacheMeasurement.Tags.Keys);
    }

    private sealed record MetricMeasurement<T>(
        string InstrumentName,
        T Value,
        IReadOnlyDictionary<string, object?> Tags);
}
