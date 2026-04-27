using Common.Presentation.Exceptions;
using Common.Presentation.Logging;
using Common.Shared;
using Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Common.Presentation;

public static class CommonPresentationRegistration
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCommonPresentationLayer(CommonTelemetryOptions? telemetryOptions = null)
        {
            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName);

                if (ActivityTraceLogEnricher.IsEnabled(ctx.Configuration))
                {
                    cfg.Enrich.With<ActivityTraceLogEnricher>();
                }
            });

            builder.Services.AddSharedLayer(builder.Configuration);
            builder.Services.AddCommonTelemetry(
                builder.Configuration,
                builder.Environment.ApplicationName,
                builder.Environment.EnvironmentName,
                telemetryOptions);

            return builder;
        }
    }

    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseDefaultExceptionHandler()
            => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
