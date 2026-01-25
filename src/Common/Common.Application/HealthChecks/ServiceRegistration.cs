using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
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

    // UseHealthChecks vs MapHealthChecks:
    // https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0#usehealthchecks-vs-maphealthchecks
    public static IEndpointRouteBuilder MapCommonHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        return endpoints;
    }
}