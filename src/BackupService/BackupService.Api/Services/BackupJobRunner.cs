using BackupService.Api.Models;
using BackupService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackupService.Api.Services;

public sealed class BackupJobRunner(
    IBackupJobStore store,
    IServiceScopeFactory scopeFactory,
    ILogger<BackupJobRunner> logger) : IBackupJobRunner
{
    public Task<string> StartSqlBackupAsync(string? backupName, CancellationToken cancellationToken = default)
    {
        var jobId = Guid.NewGuid().ToString("N");
        store.Create(jobId);

        _ = Task.Run(async () =>
        {
            store.Update(jobId, status =>
            {
                status.State = BackupJobState.Running;
                status.StartedAtUtc = DateTime.UtcNow;
            });

            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ISqlBackupService>();

                var result = await service.CreateBackupAsync(backupName, CancellationToken.None);

                store.Update(jobId, status =>
                {
                    status.State = BackupJobState.Succeeded;
                    status.CompletedAtUtc = DateTime.UtcNow;
                    status.Result = result;
                });
            }
            catch (Exception ex)
            {
                store.Update(jobId, status =>
                {
                    status.State = BackupJobState.Failed;
                    status.CompletedAtUtc = DateTime.UtcNow;
                    status.Error = ex.Message;
                });

                logger.LogError(ex, "SQL backup job failed. JobId={JobId}", jobId);
            }
        }, CancellationToken.None);

        return Task.FromResult(jobId);
    }

    public BackupJobStatus? GetStatus(string jobId)
        => store.TryGet(jobId, out var status) ? status : null;
}
