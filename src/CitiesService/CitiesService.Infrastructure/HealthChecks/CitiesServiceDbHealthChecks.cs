using System.Collections.Generic;
using CitiesService.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CitiesService.Infrastructure.HealthChecks;

public static class CitiesServiceDbHealthChecks
{
    public static IHealthChecksBuilder AddCitiesServiceDbContextCheck(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
        => builder.AddDbContextCheck<ApplicationDbContext>(
            name,
            failureStatus,
            tags);
}
