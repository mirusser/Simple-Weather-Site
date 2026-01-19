using System;

namespace MQModels.WeatherHistory;

public class CreatedCityWeatherForecastSearch
{
    public Guid EventId { get; set; }
    public Guid GotWeatherForecastEventId { get; set; }
}