using Convey.MessageBrokers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherService.Messages.Events;
using WeatherService.Models.Dto;

namespace WeatherService.Middleware.Exceptions
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly IBusPublisher _publisher;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger,
            IBusPublisher publisher)
        {
            _next = next;
            _logger = logger;
            _publisher = publisher;
        }

        public RequestDelegate Next => _next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await Next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var defaultErrorCode = "error";
            var exceptionType = exception.GetType();

            (HttpStatusCode statusCode, string errorCode) = exception switch
            {
                Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
                ServiceException e when exceptionType == typeof(ServiceException) => (HttpStatusCode.BadRequest, e.Code),
                _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
            };

            _logger.LogError("WeatherService: Exception code: {ErrorCode}, Exception message: {ExceptionMessage}", new[] { errorCode, exception.Message });

            await _publisher.PublishAsync(new SendEmailRequestEvent(new SendEmailDto() { Subject = $"Exception from WeatherService", Body = $"{exception.Message} {exception.StackTrace}"}));

            var response = new { code = errorCode, message = exception.Message };
            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(payload);
        }
    }
}
