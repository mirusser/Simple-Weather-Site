using System.Collections.Concurrent;
using System.Reflection;
using Common.ExtensionMethods;
using Common.Mediator;
using Common.Mediator.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropsCache = new();

    /// <summary>
    /// Logs basic information about the incoming mediator request and the produced response.
    /// 
    /// This behavior intentionally uses reflection to enumerate and log the request object's public properties.
    /// Reflection is slower than direct access and can be noisy (and risky if you log sensitive/large values),
    /// but it’s useful here to demonstrate how pipeline behaviors can inspect requests generically without
    /// knowing their concrete types at compile time.
    /// 
    /// After logging the request details, it invokes the next handler in the pipeline and then logs the
    /// response type using a friendly generic type formatter.
    /// </summary>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestType = request.GetType(); // use runtime type intentionally
        logger.LogInformation("Handling request of type: {TypeName}", requestType.Name);

        var props = PropsCache.GetOrAdd(requestType, t => t.GetProperties());

        if (props.Length > 0)
        {
            logger.LogInformation("Found {Count} properties: ", props.Length);
        }

        foreach (var prop in props)
        {
            // Skip problematic stuff 
            if (prop.GetIndexParameters().Length > 0) // indexers
            {
                continue;
            }

            object? value;
            try
            {
                value = prop.GetValue(request);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to read property {Property}", prop.Name);
                continue;
            }

            // Avoids dumping large / sensitive values
            if (value is IFormFile or IFormFileCollection)
            {
                logger.LogInformation("{Property}: [file omitted]", prop.Name);
                continue;
            }

            logger.LogInformation("{Property}: {@Value}", prop.Name, value);
        }

        var response = await next();

        logger.LogInformation(
            "Handled request with response type: {TypeName}",
            typeof(TResponse).ToFriendlyFormat());

        return response;
    }
}