using System.Reflection;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : IErrorOr
{
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
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