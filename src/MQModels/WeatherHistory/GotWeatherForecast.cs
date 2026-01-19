using System;

namespace MQModels.WeatherHistory;

public record GotWeatherForecast(
    Guid EventId,
    string City,
    string CountryCode,
    DateTimeOffset Date,
    int TemperatureC,
    int TemperatureF,
    string Summary,
    string Icon);