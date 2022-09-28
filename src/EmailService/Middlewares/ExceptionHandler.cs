using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.Internal;

namespace EmailService.Middlewares;

public class ExceptionHandler : IMiddleware
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var defaultErrorCode = "error";
        var exceptionType = exception.GetType();

        (HttpStatusCode statusCode, string errorCode) = exception switch
        {
            Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
            CustomException e when exceptionType == typeof(CustomException) => (HttpStatusCode.InternalServerError, e.Code),
            _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
        };

        _logger.LogError(
            "Exception code: {ErrorCode}, Exception message: {ExceptionMessage}, Stack trace: {StackTrace}",
            new[]
            {
                    errorCode,
                    exception.InnerException != null ? exception.InnerException.Message : exception.Message,
                    exception.StackTrace });

        var response = new Response<object>()
        {
            IsSuccess = false,
            Data = null,
            Message = errorCode,
            Errors = new List<string>() { exception.InnerException != null ? exception.InnerException.Message : exception.Message }
        };

        var payload = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(payload);
    }
}