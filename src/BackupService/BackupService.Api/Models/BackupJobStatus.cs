using BackupService.Application.Models;

namespace BackupService.Api.Models;

public sealed class BackupJobStatus
{
    public string JobId { get; set; } = null!;
    public BackupJobState State { get; set; } = BackupJobState.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? Error { get; set; }
    public SqlBackupResult? Result { get; set; }
}
