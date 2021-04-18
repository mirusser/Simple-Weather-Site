using CitiesService.Logic.Managers.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CitiesService.Logic.HealthChecks
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly ICityManager _cityManager;

        public CustomHealthCheck(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
        {
            try
            {
                var result = _cityManager.GetCitiesPagination(1, 1);

                return result.Cities != null && result.Cities.Any() ?
                    Task.FromResult(HealthCheckResult.Healthy("The 'CitiesService' is healthy")) :
                    throw new Exception("There aren't any cities in the database");

            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(ex.Message));
            }
        }
    }
}
