namespace BackupService.Application.Models.Results;

public sealed class SqlBackupResult
{
    public string DatabaseName { get; set; } = null!;
    public string BackupFilePath { get; set; } = null!;
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset CompletedAtUtc { get; set; }
    public long? SizeBytes { get; set; }
}
