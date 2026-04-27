using System.Diagnostics;

namespace CitiesService.GraphQL;

public static class GraphQlTelemetry
{
    public const string ActivitySourceName = "CitiesService.GraphQL";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public static Activity? StartActivity(string operation)
    {
        var activity = ActivitySource.StartActivity(operation);
        activity?.SetTag("operation", operation);

        return activity;
    }

    public static void SetResult(Activity? activity, string result)
    {
        activity?.SetStatus(result is "success" or "not_found" or "deferred"
            ? ActivityStatusCode.Ok
            : ActivityStatusCode.Error);
        activity?.SetTag("result", result);
    }

    public static void SetException(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        activity?.SetTag("result", "exception");
        activity?.SetTag("error_type", exception.GetType().Name);
    }
}
