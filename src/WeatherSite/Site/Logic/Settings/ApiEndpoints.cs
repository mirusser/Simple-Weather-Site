namespace WeatherSite.Logic.Settings;

public class ApiEndpoints
{
    public required string WeatherServiceApiUrl { get; set; }
    public required string CitiesServiceApiUrl { get; set; }
    public required string WeatherHistoryServiceApiUrl { get; set; }
    public required string IconServiceApiUrl { get; set; }
    public required string SignalRServer { get; set; }
}