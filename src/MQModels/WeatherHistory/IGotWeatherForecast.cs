using System;

namespace MQModels.WeatherHistory;

public interface IGotWeatherForecast
{
    Guid EventId { get; }
    string City { get; }
    string CountryCode { get; }
    DateTimeOffset Date { get; }
    int TemperatureC { get; }
    int TemperatureF { get; }
    string Summary { get; }
    string Icon { get; }
}