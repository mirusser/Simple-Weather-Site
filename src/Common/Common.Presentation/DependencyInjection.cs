using Common.Presentation.Exceptions;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonPresentationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedLayer(configuration);

        return services;
    }

    public static IApplicationBuilder UseServiceExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware(typeof(ExceptionHandlerMiddleware));
}