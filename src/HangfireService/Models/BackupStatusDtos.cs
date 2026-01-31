namespace HangfireService.Models;

public sealed class StartSqlBackupResultDto
{
    public string JobId { get; set; } = null!;
}

public sealed class GetSqlBackupStatusResultDto
{
    public BackupJobStatusDto? Status { get; set; }
}

public sealed class BackupJobStatusDto
{
    public string JobId { get; set; } = null!;
    public BackupJobState State { get; set; }
    public string? Error { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public enum BackupJobState
{
    Pending,
    Running,
    Succeeded,
    Failed
}
