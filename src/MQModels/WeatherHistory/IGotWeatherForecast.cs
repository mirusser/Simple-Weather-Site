using System;

namespace MQModels.WeatherHistory;

public interface IGotWeatherForecast
{
    string City { get; set; }
    string CountryCode { get; set; }
    DateTime Date { get; set; }
    int TemperatureC { get; set; }
    int TemperatureF { get; set; }
    string Summary { get; set; }
    string Icon { get; set; }
}