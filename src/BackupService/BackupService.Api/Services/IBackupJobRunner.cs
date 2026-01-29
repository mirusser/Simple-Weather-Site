using BackupService.Api.Models;

namespace BackupService.Api.Services;

public interface IBackupJobRunner
{
    Task<string> StartSqlBackupAsync(string? backupName, CancellationToken cancellationToken = default);
    BackupJobStatus? GetStatus(string jobId);
}
