using System.Net;
using System.Text.Json;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Presentation.Exceptions;

public class ExceptionHandlerMiddleware(
	RequestDelegate next,
	ILogger<ExceptionHandlerMiddleware> logger)
{
	private readonly JsonSerializerOptions jsonSerializerOptions
		= new() { PropertyNameCaseInsensitive = true };

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
		var (statusCode, errorCode) = exception switch
		{
			UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ErrorCodes.Unauthorized),
			ServiceException e => (HttpStatusCode.BadRequest, e.Code),
			HttpRequestException => (HttpStatusCode.ServiceUnavailable, ErrorCodes.Service_Unavailable),
			ValidationException => (HttpStatusCode.BadRequest, ErrorCodes.ValidationException),
			_ => (HttpStatusCode.InternalServerError, ErrorCodes.DefaultError),
		};

		logger.LogError(
			exception,
			"Exception code: {ErrorCode}, Exception message: {Message}",
			errorCode,
			exception.Message);

		var response = Error.Failure(code: errorCode, description: exception.Message);
		var payload = JsonSerializer.Serialize(response, jsonSerializerOptions);

		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		await context.Response.WriteAsync(payload);
	}
}