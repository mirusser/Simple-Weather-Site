namespace BackupService.Application.Settings;

public sealed class BackupSettings
{
    public string? DatabaseName { get; set; }
    public string? BackupDirectory { get; set; }
    public int RetentionDays { get; set; } = 7;
    public int CommandTimeoutSeconds { get; set; } = 1800;
    public string? FilePrefix { get; set; }
    public bool UseCompression { get; set; } = true;
    public bool UseCopyOnly { get; set; } = true;
    public bool SkipLocalDirectoryCreation { get; set; } = true;
}
