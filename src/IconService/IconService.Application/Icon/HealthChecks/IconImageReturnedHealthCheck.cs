using Common.Contracts.HealthCheck;
using Common.Mediator;
using IconService.Application.Icon.Queries.Get;
using IconService.Domain.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IconService.Application.Icon.HealthChecks;

public sealed class IconImageReturnedHealthCheck(
    IMediator mediator,
    IOptions<ServiceSettings> options,
    ILogger<IconImageReturnedHealthCheck> logger) : IHealthCheck
{
    private readonly ServiceSettings serviceSettings = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await mediator.SendAsync(
                new GetQuery(serviceSettings.HealthCheckIcon),
                cancellationToken);

            if (!string.Equals(result.Icon, serviceSettings.HealthCheckIcon, StringComparison.Ordinal) ||
                result.FileContent is not { Length: > 0 })
            {
                return HealthCheckResult.Unhealthy(
                    "Icon payload is missing or invalid.",
                    data: new Dictionary<string, object>
                    {
                        ["icon"] = serviceSettings.HealthCheckIcon
                    });
            }

            return HealthCheckResult.Healthy(
                $"Icon '{serviceSettings.HealthCheckIcon}' returned a non-empty image.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "In {HealthCheck}", nameof(IconImageReturnedHealthCheck));

            return HealthCheckResult.Unhealthy(nameof(IconImageReturnedHealthCheck), ex);
        }
    }
}
