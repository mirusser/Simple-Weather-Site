using CitiesService.Application.Features.HealthChecks;
using CitiesService.Infrastructure.Contexts;
using Common.Application.HealthChecks;
using Common.Contracts.HealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CitiesService.Api;

public static class ServiceRegistration
{
	public static IServiceCollection AddPresentation(this IServiceCollection services)
	{
		services.AddControllers();
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

		services.AddCommonHealthChecks()
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