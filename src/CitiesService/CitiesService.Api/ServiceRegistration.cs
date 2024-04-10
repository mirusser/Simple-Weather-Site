using CitiesService.Application.Features.HealthChecks;
using CitiesService.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Common.Contracts.HealthCheck;

namespace CitiesService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CitiesService", Version = "v1" });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
        });

        services.AddHttpClient();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "SQL health check",
                failureStatus: HealthStatus.Unhealthy,
                tags: [nameof(HealthChecksTags.Database)])
            .AddCheck<CitiesAvailableHealthCheck>(
                name: "Cities available health check",
				failureStatus: HealthStatus.Degraded,
				tags: [nameof(HealthChecksTags.Database)]);

		return services;
    }
}