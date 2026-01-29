using BackupService.Api.Models;

namespace BackupService.Api.Services;

public interface IBackupJobStore
{
    BackupJobStatus Create(string jobId);
    bool TryGet(string jobId, out BackupJobStatus status);
    void Update(string jobId, Action<BackupJobStatus> update);
}
