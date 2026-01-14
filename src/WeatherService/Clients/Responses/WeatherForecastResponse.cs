namespace WeatherService.Clients.Responses;

public record Forecast(
    Coord coord,
    Weather[] weather,
    Main main,
    int visibility,
    Wind wind,
    Clouds clouds,
    long dt,
    Sys sys,
    int timezone,
    int id,
    string name,
    int cod);

public record Coord(decimal lon, decimal lat); //coordinates - longitude, latitude

public record Weather(int id, string main, string description, string icon);

public record Main(
    decimal temp,
    decimal feels_like,
    decimal temp_min,
    decimal temp_max,
    int pressure,
    int humidity);

public record Wind(decimal speed, int deg);

public record Clouds(int all);

public record Sys(int type, int id, string country, long sunrise, long sunset);

