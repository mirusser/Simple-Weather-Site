using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using Common.Mediator.Wrappers;

namespace CitiesService.Application.Telemetry;

public sealed class CitiesTelemetryBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var operation = request.GetType().Name;
        using var activity = CitiesTelemetry.StartApplicationActivity(operation);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await next();
            var result = GetResult(response);

            activity?.SetStatus(result == CitiesTelemetryConventions.ResultValues.Success
                ? ActivityStatusCode.Ok
                : ActivityStatusCode.Error);
            activity?.SetTag(CitiesTelemetryConventions.TagNames.Result, result);
            CitiesTelemetry.RecordMediatorRequest(operation, Stopwatch.GetElapsedTime(startedAt), result);

            return response;
        }
        catch (Exception ex)
        {
            var errorType = ex.GetType().Name;

            activity?.SetStatus(ActivityStatusCode.Error, errorType);
            activity?.SetTag(CitiesTelemetryConventions.TagNames.Result, CitiesTelemetryConventions.ResultValues.Exception);
            activity?.SetTag(CitiesTelemetryConventions.TagNames.ErrorType, errorType);
            CitiesTelemetry.RecordMediatorRequest(
                operation,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Exception,
                errorType);

            throw;
        }
    }

    private static string GetResult(TResponse response)
    {
        var isSuccessProperty = typeof(TResponse).GetProperty("IsSuccess");
        if (response is not null
            && isSuccessProperty?.PropertyType == typeof(bool)
            && isSuccessProperty.GetValue(response) is bool isSuccess)
        {
            return isSuccess
                ? CitiesTelemetryConventions.ResultValues.Success
                : CitiesTelemetryConventions.ResultValues.Failure;
        }

        return CitiesTelemetryConventions.ResultValues.Success;
    }
}
