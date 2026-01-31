namespace BackupService.Application.Models.Requests;

public sealed class StartSqlBackupRequest
{
    public string? BackupName { get; set; }
}
