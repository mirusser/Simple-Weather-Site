using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MQModels.Email;

namespace CitiesService.Exceptions
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        //private readonly IPublishEndpoint _publishEndpoint;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger
            )
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IPublishEndpoint publishEndpoint)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, publishEndpoint);
            }
        }

        private async Task<Task> HandleExceptionAsync(HttpContext context, Exception exception, IPublishEndpoint publishEndpoint)
        {
            var defaultErrorCode = "error";
            var exceptionType = exception.GetType();

            (HttpStatusCode statusCode, string errorCode) = exception switch
            {
                Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
                Exception when exceptionType == typeof(SqlException) => (HttpStatusCode.NotFound, ErrorCodes.SqlException),
                ServiceException e when exceptionType == typeof(ServiceException) => (HttpStatusCode.BadRequest, e.Code),
                _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
            };

            _logger.LogError("CitiesService: Exception code: {ErrorCode} Exception message: {ExceptionMEssage}", new[] { errorCode, exception.Message });

            var sendEmail = new SendEmail()
            {
                Subject = "Exception",
                Body = exception.InnerException != null ? exception.InnerException.ToString() : exception.Message
            };
            await publishEndpoint.Publish(sendEmail);

            var response = new { code = errorCode, message = exception.Message };
            var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(payload);
        }
    }
}