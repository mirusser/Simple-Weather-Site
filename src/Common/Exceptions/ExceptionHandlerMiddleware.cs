using System.Net;
using System.Text.Json;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Presentation.Exceptions;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger
        )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task<Task> HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var defaultErrorCode = "error";
        var exceptionType = exception.GetType();

        (HttpStatusCode statusCode, string errorCode) = exception switch
        {
            Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
            ServiceException e when exceptionType == typeof(ServiceException) => (HttpStatusCode.BadRequest, e.Code),
            ValidationException when exceptionType == typeof(ValidationException) => (HttpStatusCode.BadRequest, ErrorCodes.ValidationException),
            _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
        };

        _logger.LogError("Exception code: {ErrorCode} Exception message: {ExceptionMessage}", new[] { errorCode, exception.Message });

        var response = Error.Failure(
            code: errorCode,
            description: exception.Message);

        var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return await Task.FromResult(context.Response.WriteAsync(payload));
    }
}