using Common.Contracts.HealthCheck;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Common.Application.HealthChecks;

public static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IHealthChecksBuilder AddCommonHealthChecks(IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();

            // Liveness
            builder.AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: [HealthChecksTags.Live]);

            var redisConn = configuration.GetConnectionString(nameof(ConnectionStrings.RedisConnection));
            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                builder.AddRedis(
                    redisConn,
                    name: "redis",
                    failureStatus: HealthStatus.Degraded, // cache is optional
                    tags: [HealthChecksTags.Ready, HealthChecksTags.Cache],
                    timeout: TimeSpan.FromSeconds(2));
            }

            return builder;
        }
    }

    extension(IHealthChecksBuilder builder)
    {
        public IHealthChecksBuilder AddExternalEndpointHealthCheck(string name,
            ExternalEndpointHealthCheckOptions options,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
        {
            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new ExternalEndpointHealthCheck(
                    sp.GetRequiredService<IHttpExecutor>(),
                    sp.GetRequiredService<ILogger<ExternalEndpointHealthCheck>>(),
                    options),
                failureStatus,
                tags,
                timeout));
        }
    }

    // UseHealthChecks vs MapHealthChecks:
    // https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0#usehealthchecks-vs-maphealthchecks
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapCommonHealthChecks()
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();

            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(HealthChecksTags.Live)
            }).AllowAnonymous();

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(HealthChecksTags.Ready),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();

            return endpoints;
        }
    }
}