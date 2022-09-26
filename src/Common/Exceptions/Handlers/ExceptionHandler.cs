using Microsoft.AspNetCore.Builder;

namespace Common.Presentation.Exceptions.Handlers;

public static class ExceptionHandler
{
    public static IApplicationBuilder UseServiceExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware(typeof(ExceptionHandlerMiddleware));
}