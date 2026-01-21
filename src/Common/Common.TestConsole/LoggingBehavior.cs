using System.Reflection;
using Common.Mediator;
using Common.Mediator.Wrappers;
using Microsoft.Extensions.Logging;

namespace Common.TestConsole;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        //Request
        logger.LogInformation("Handling request of type: {TypeName}", typeof(TRequest).Name);
        Type myType = request.GetType();

        List<PropertyInfo> props = new(myType.GetProperties());

        foreach (PropertyInfo prop in props)
        {
            object? propValue = prop.GetValue(request, null);
            logger.LogInformation("{Property} : {@Value}", prop.Name, propValue);
        }

        var response = await next();

        //Response
        logger.LogInformation("Handled request with response type: {TypeName}", typeof(TResponse).FullName);

        return response;
    }
}