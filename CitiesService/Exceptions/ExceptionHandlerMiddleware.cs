using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CitiesService.Exceptions
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var defaultErrorCode = "error";
            var exceptionType = exception.GetType();

            (HttpStatusCode statusCode, string errorCode) = exception switch
            {
                Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
                ServiceException e when exceptionType == typeof(ServiceException) => (HttpStatusCode.BadRequest, e.Code),
                _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
            };

            var response = new { code = errorCode, message = exception.Message };
            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(payload);
        }
    }
}
