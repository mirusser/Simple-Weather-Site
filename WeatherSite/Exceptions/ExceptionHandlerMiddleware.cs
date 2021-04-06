﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherSite.Exceptions
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var defaultErrorCode = "error";
            var exceptionType = exception.GetType();

            (HttpStatusCode statusCode, string errorCode) = exception switch
            {
                Exception when exceptionType == typeof(UnauthorizedAccessException) => (HttpStatusCode.Unauthorized, defaultErrorCode),
                Exception when exceptionType == typeof(HttpRequestException) => (HttpStatusCode.ServiceUnavailable, ErrorCodes.Service_Unavailable),
                SiteException e when exceptionType == typeof(SiteException) => (HttpStatusCode.BadRequest, e.Code),
                _ => (HttpStatusCode.InternalServerError, defaultErrorCode),
            };

            _logger.LogError($"{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}: Exception code: {errorCode} Exception message: {exception.Message}");

            var response = new { code = errorCode, message = exception.Message };
            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(payload);
        }
    }
}
