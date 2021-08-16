using Application.Interfaces.Managers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.HealthChecks
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly ICityManager _cityManager;

        public CustomHealthCheck(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _cityManager.GetCitiesPaginationDto(1, 1);

                return result.Cities != null && result.Cities.Any() ?
                    HealthCheckResult.Healthy("The 'CitiesService' is healthy") :
                    throw new Exception("There aren't any cities in the database");

            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
