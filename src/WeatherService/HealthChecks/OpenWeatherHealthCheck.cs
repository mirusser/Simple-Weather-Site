using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Presentation.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherService.Clients;
using WeatherService.Clients.Responses;
using WeatherService.Settings;

namespace WeatherService.HealthChecks;

public sealed class OpenWeatherHealthCheck(
    WeatherClient weatherClient,
    IOptions<ServiceSettings> options,
    ILogger<OpenWeatherHealthCheck> logger) : IHealthCheck
{
    private readonly ServiceSettings serviceSettings = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Result<Forecast> result = await weatherClient.GetCurrentWeatherByCityIdAsync(
                serviceSettings.HealthCheckCityId,
                cancellationToken);

            if (!result.IsSuccess)
            {
                var problem = result.Problem!;
                return HealthCheckResult.Unhealthy(
                    problem.Message,
                    data: new Dictionary<string, object>
                    {
                        ["code"] = problem.Code,
                        ["status"] = problem.Status,
                        ["cityId"] = serviceSettings.HealthCheckCityId
                    });
            }

            var forecast = result.Value;
            if (forecast is null ||
                forecast.id <= 0 ||
                string.IsNullOrWhiteSpace(forecast.name) ||
                forecast.weather is not { Length: > 0 })
            {
                return HealthCheckResult.Unhealthy(
                    "OpenWeather returned an incomplete forecast payload.",
                    data: new Dictionary<string, object>
                    {
                        ["cityId"] = serviceSettings.HealthCheckCityId
                    });
            }

            return HealthCheckResult.Healthy(
                $"OpenWeather returned weather for city '{forecast.name}' ({serviceSettings.HealthCheckCityId}).");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "In {HealthCheck}", nameof(OpenWeatherHealthCheck));

            return HealthCheckResult.Unhealthy(nameof(OpenWeatherHealthCheck), ex);
        }
    }
}
