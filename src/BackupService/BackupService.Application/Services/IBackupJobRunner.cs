using BackupService.Application.Models;

namespace BackupService.Application.Services;

public interface IBackupJobRunner
{
    Task<string> StartSqlBackupAsync(string? backupName, CancellationToken cancellationToken = default);
    BackupJobStatus? GetStatus(string jobId);
}
