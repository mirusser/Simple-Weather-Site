using BackupService.Application.Models;

namespace BackupService.Application.Services;

public interface IBackupJobStore
{
    BackupJobStatus Create(string jobId);
    bool TryGet(string jobId, out BackupJobStatus status);
    void Update(string jobId, Action<BackupJobStatus> update);
}
