using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherService.Settings;

namespace WeatherService.HealthCheck;

public class OpenWeatherExternalEndpointHealthCheck(
	IOptions<ServiceSettings> options,
	ILogger<OpenWeatherExternalEndpointHealthCheck> logger) : IHealthCheck
{
	private readonly ServiceSettings serviceSettings = options.Value;

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Ping ping = new();

			var reply = await ping.SendPingAsync(serviceSettings.OpenWeatherHost);

			return reply.Status != IPStatus.Success
				? HealthCheckResult.Degraded(reply.Status.ToString())
				: HealthCheckResult.Healthy("Ready");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"In {nameof(OpenWeatherExternalEndpointHealthCheck)}");

			return HealthCheckResult.Unhealthy(ex.Message, ex);
		}
	}
}