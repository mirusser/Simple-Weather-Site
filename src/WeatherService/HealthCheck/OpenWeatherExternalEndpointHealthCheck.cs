using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace WeatherService.HealthCheck;

public sealed class OpenWeatherExternalEndpointHealthCheck(
	IHttpExecutor httpExecutor,
	ILogger<OpenWeatherExternalEndpointHealthCheck> logger) : IHealthCheck
{
	private const string ClientName = "OpenWeather";
	private const string PipelineName = PipelineNames.Health;

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Head, new Uri("/", UriKind.Relative));
			using var response = await httpExecutor.SendAsync(ClientName, PipelineName, request, cancellationToken);

			// If we got an HTTP response at all, DNS+TLS+HTTP path works.
			return HealthCheckResult.Healthy($"Reachable (HTTP {(int)response.StatusCode})");
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			return HealthCheckResult.Degraded("Timed out contacting OpenWeather");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "In {HealthCheck}", nameof(OpenWeatherExternalEndpointHealthCheck));
			return HealthCheckResult.Unhealthy(ex.Message, ex);
		}
	}
}