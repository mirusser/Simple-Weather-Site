using Common.Presentation.Exceptions;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddCommonPresentationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedLayer(configuration);

        return services;
    }

    public static IApplicationBuilder UseDefaultExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware(typeof(ExceptionHandlerMiddleware));
}