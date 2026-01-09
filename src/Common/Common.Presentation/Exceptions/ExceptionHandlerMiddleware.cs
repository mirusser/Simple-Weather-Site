using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Common.Domain.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.Presentation.Exceptions;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Invoke(HttpContext context)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, problem) = exception switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ErrorCodes.Unauthorized,
                new ProblemDetails { Title = "Unauthorized", Detail = exception.Message }
            ),

            // Your “expected” app exception (see Step 4)
            ServiceException se => (
                HttpStatusCode.BadRequest,
                se.Code,
                new ProblemDetails { Title = se.Message, Detail = se.Message }
            ),

            HttpRequestException => (
                HttpStatusCode.ServiceUnavailable,
                ErrorCodes.ServiceUnavailable,
                new ProblemDetails { Title = "Service Unavailable", Detail = exception.Message }
            ),

            ValidationException ve => (
                HttpStatusCode.BadRequest,
                ErrorCodes.Validation,
                ToValidationProblemDetails(ve)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCodes.DefaultError,
                new ProblemDetails { Title = "Internal Server Error", Detail = "An unexpected error occurred." }
            )
        };

        logger.LogError(
            exception,
            "Exception code: {ErrorCode}, Exception message: {Message}",
            errorCode,
            exception.Message);

        // Enrich problem details (standard goodies)
        problem.Status = (int)statusCode;
        problem.Instance = context.Request.Path;
        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static ValidationProblemDetails ToValidationProblemDetails(ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed"
        };
    }
}