using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MQModels.Email;

namespace WeatherService.Middleware.Exceptions
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public RequestDelegate Next => _next;

        public async Task Invoke(HttpContext context, IPublishEndpoint publishEndpoint)
        {
            try
            {
                await Next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, publishEndpoint);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, IPublishEndpoint publishEndpoint)
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

            var sendEmail = new SendEmail()
            {
                Subject = "Exception",
                Body = exception.InnerException != null ? exception.InnerException.ToString() : exception.Message
            };
            await publishEndpoint.Publish(sendEmail);

            var response = new { code = errorCode, message = exception.Message };
            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(payload);
        }
    }
}