using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Application.HealthChecks;

public static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IHealthChecksBuilder AddCommonHealthChecks()
        {
            var builder = services.AddHealthChecks();

            // Liveness
            builder.AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live"]);

            return builder;
        }
    }

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseCommonHealthChecks()
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }
    }
}