namespace BackupService.Application.Services;

public interface ISqlBackupService
{
    Task<Models.SqlBackupResult> CreateBackupAsync(
        string? backupName = null,
        CancellationToken cancellationToken = default);
}
