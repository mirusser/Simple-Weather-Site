namespace Common.Infrastructure.Settings;

public class ResiliencePipelines
{
    public ResiliencePipelineOptions Default { get; set; } = new();
    public ResiliencePipelineOptions Health { get; set; } = new();
}

public class ResiliencePipelineOptions
{
    public string Name { get; set; } = PipelineNames.Default;
    public int MaxRetryAttempts { get; set; } = 3;
    public int DelaySeconds { get; set; } = 2;
    public bool UseJitter { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 5;
}

public static class PipelineNames
{
    public const string Default = "default";
    public const string Health = "health";
}