namespace HangfireService.Settings;

public sealed class BackupJobSettings
{
    public bool Enabled { get; set; } = true;
    public string JobName { get; set; } = "sql-backup-nightly";
    public string CronExpression { get; set; } = "0 2 * * *";
    public string StartUrl { get; set; } = null!;
    public string StatusUrl { get; set; } = null!;
    public string? BackupName { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public int MaxPollAttempts { get; set; } = 60;
}
