using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.PipelineBehaviours
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogInformation($"Handling {typeof(TRequest).Name}");

            if (request is not null)
            {
                Type myType = request.GetType();
                var propertyInfos = new List<PropertyInfo>(myType.GetProperties());

                foreach (var propertyInfo in propertyInfos)
                {
                    object? propertyValue = propertyInfo.GetValue(request, null);
                    _logger.LogInformation("{Property} : {@Value}", propertyInfo.Name, propertyValue);
                }
            }

            var response = await next();

            _logger.LogInformation($"Handled {typeof(TResponse).Name}");

            return response;
        }
    }
}