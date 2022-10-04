using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WeatherService.Settings;

namespace WeatherService.HealthCheck;

public class ExternalEndpointHealthCheck : IHealthCheck
{
    private readonly ServiceSettings _serviceSettings;

    public ExternalEndpointHealthCheck(IOptions<ServiceSettings> options)
    {
        _serviceSettings = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        Ping ping = new();

        var reply = await ping.SendPingAsync(_serviceSettings.OpenWeatherHost);

        return reply.Status != IPStatus.Success ?
            HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
    }
}