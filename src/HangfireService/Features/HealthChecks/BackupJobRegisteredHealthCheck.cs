using Hangfire;
using Hangfire.Storage;
using HangfireService.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HangfireService.Features.HealthChecks;

public sealed class BackupJobRegisteredHealthCheck(IOptions<BackupJobSettings> options)
    : IHealthCheck
{
    private readonly BackupJobSettings settings = options.Value;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!settings.Enabled)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Backup job is disabled."));
        }

        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();

        var exists = jobs.Any(j => string.Equals(j.Id, settings.JobName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(
            exists
                ? HealthCheckResult.Healthy("Backup job is registered.")
                : HealthCheckResult.Degraded("Backup job is not registered."));
    }
}
