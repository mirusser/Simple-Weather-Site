using Common.Infrastructure.Managers.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Common.Application.HealthChecks;

public sealed class ExternalEndpointHealthCheck(
    IHttpExecutor httpExecutor,
    ILogger<ExternalEndpointHealthCheck> logger,
    ExternalEndpointHealthCheckOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(options.Method, options.Target);
            using var response = await httpExecutor.SendAsync(
                options.ClientName,
                options.PipelineName,
                request,
                cancellationToken);

            if (options.AnyHttpStatusIsHealthy)
            {
                return HealthCheckResult.Healthy(
                    options.HealthyMessage ?? $"Reachable (HTTP {(int)response.StatusCode})");
            }

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(options.HealthyMessage ?? $"OK (HTTP {(int)response.StatusCode})")
                : HealthCheckResult.Degraded($"HTTP {(int)response.StatusCode}");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Degraded($"Timed out contacting endpoint: {options.Target}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "External endpoint health check failed: {url}", options.Target);
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
