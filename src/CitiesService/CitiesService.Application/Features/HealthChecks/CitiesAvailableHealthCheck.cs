using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using Common.Mediator;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Features.HealthChecks;

public class CitiesAvailableHealthCheck(
	IMediator mediator,
	ILogger<CitiesAvailableHealthCheck> logger) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			GetCitiesPaginationQuery query = new() { NumberOfCities = 1, PageNumber = 1 };
			var result = await mediator.SendAsync(query, cancellationToken);

			return result is { IsSuccess: true, Value.Cities.Count: > 0 }
				? HealthCheckResult.Healthy("The 'CitiesService' is healthy")
				: throw new Exception("There aren't any cities in the database");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "In {HealthCheck}", nameof(CitiesAvailableHealthCheck));

			return HealthCheckResult.Unhealthy(nameof(CitiesAvailableHealthCheck), ex);
		}
	}
}