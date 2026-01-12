using Common.Presentation.Exceptions;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Common.Presentation;

public static class CommonPresentationRegistration
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCommonPresentationLayer()
        {
            builder.Host.UseSerilog((ctx, services, cfg) =>
                cfg.ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

            builder.Services.AddCommonPresentationLayer(builder.Configuration);
            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommonPresentationLayer(IConfiguration configuration)
        {
            services.AddSharedLayer(configuration);

            return services;
        }
    }

    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseDefaultExceptionHandler()
            => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}