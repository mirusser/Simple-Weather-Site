namespace BackupService.Application.Models.Results;

public class GetSqlBackupStatusResult
{
    public BackupJobStatus? Status { get; set; }
}