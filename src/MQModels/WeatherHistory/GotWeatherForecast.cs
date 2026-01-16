using System;

namespace MQModels.WeatherHistory;

public record GotWeatherForecast(
    Guid EventId,
    string City,
    string CountryCode,
    DateTime ObservedAtUtc,
    int TemperatureC,
    int TemperatureF,
    string Summary,
    string Icon);