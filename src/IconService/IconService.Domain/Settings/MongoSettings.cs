namespace IconService.Domain.Settings;

public class MongoSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Database { get; set; } = null!;
    public string IconsCollectionName{ get; set; } = null!;
    public bool Seed { get; set; }
}