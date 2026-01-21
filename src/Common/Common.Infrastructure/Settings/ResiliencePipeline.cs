namespace Common.Infrastructure.Settings;

public class ResiliencePipeline
{
    public string Name { get; set; } = "resilience-pipeline";
    public int MaxRetryAttempts { get; set; } = 3;
    public int DelaySeconds { get; set; } = 2;
    public bool UseJitter { get; set; } = true;
    public int DefaultTimeoutSeconds { get; set; } = 5;
}