namespace IconService.Domain.Settings;

public class MongoSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Database { get; set; } = null!;
    public string IconsCollectionName{ get; set; } = null!;
    public SeedingOptions SeedingSettings { get; set; } = null!;
    
    public sealed class SeedingOptions
    {
        public bool Enabled { get; init; } = true;
        public bool OnlyInDevelopment { get; init; } = true;
    }

}