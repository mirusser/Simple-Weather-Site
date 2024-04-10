using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using MediatR;
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
			var result = await mediator.Send(query, cancellationToken);

			return result.Value.Cities?.Count > 0
				? HealthCheckResult.Healthy("The 'CitiesService' is healthy")
				: throw new Exception("There aren't any cities in the database");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"In {nameof(CitiesAvailableHealthCheck)}");

			return HealthCheckResult.Unhealthy(ex.Message);
		}
	}
}