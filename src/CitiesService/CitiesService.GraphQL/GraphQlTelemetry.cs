using System.Diagnostics;
using CitiesService.Application.Telemetry;

namespace CitiesService.GraphQL;

public static class GraphQlTelemetry
{
    public const string ActivitySourceName = CitiesTelemetryConventions.ActivitySources.GraphQl;

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static Activity? StartActivity(string operation)
    {
        var activity = ActivitySource.StartActivity(operation);
        activity?.SetTag(CitiesTelemetryConventions.TagNames.Operation, operation);

        return activity;
    }

    public static void SetResult(Activity? activity, string result)
    {
        activity?.SetStatus(result is CitiesTelemetryConventions.ResultValues.Success
                or CitiesTelemetryConventions.ResultValues.NotFound
                or CitiesTelemetryConventions.ResultValues.Deferred
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
}
