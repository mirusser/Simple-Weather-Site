namespace WeatherService.Settings;

public class MongoSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Database { get; set; } = null!;
    public OutboxConfig OutboxSettings { get; set; } = null!;
    
    public class OutboxConfig
    {
        public int QueryDelaySeconds { get; set; }
        public int DuplicateDetectionWindowSeconds { get; set; }
    }
}