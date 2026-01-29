using System.Collections.Concurrent;
using BackupService.Api.Models;

namespace BackupService.Api.Services;

public sealed class BackupJobStore : IBackupJobStore
{
    private readonly ConcurrentDictionary<string, BackupJobStatus> jobs = new();

    public BackupJobStatus Create(string jobId)
    {
        var status = new BackupJobStatus
        {
            JobId = jobId,
            State = BackupJobState.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        jobs[jobId] = status;
        return status;
    }

    public bool TryGet(string jobId, out BackupJobStatus status)
        => jobs.TryGetValue(jobId, out status!);

    public void Update(string jobId, Action<BackupJobStatus> update)
    {
        if (!jobs.TryGetValue(jobId, out var status))
        {
            return;
        }

        lock (status)
        {
            update(status);
        }
    }
}
