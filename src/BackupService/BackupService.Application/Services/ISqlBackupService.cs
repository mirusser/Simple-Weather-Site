using BackupService.Application.Models.Results;

namespace BackupService.Application.Services;

public interface ISqlBackupService
{
    Task<SqlBackupResult> CreateBackupAsync(
        string? backupName = null,
        CancellationToken cancellationToken = default);
}
