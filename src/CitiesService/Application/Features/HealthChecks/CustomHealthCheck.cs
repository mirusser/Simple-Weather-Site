using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CitiesService.Application.Features.HealthChecks;

public class CustomHealthCheck : IHealthCheck
{
    private readonly IMediator mediator;

    public CustomHealthCheck(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCitiesPaginationQuery { NumberOfCities = 1, PageNumber = 1 };
            var result = await mediator.Send(query, cancellationToken);

            return result.Value.Cities?.Any() == true
                ? HealthCheckResult.Healthy("The 'CitiesService' is healthy")
                : throw new Exception("There aren't any cities in the database");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}