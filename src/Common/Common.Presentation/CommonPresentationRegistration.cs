using Common.Presentation.Exceptions;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
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

            builder.Services.AddSharedLayer(builder.Configuration);
            
            return builder;
        }
    }

    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseDefaultExceptionHandler()
            => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}