using System;

namespace WeatherHistoryService.Models.Dto;

public class WeatherForecastDto
{
    public string City { get; set; }
    public string CountryCode { get; set; }
    public DateTimeOffset Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF { get; set; }
    public string Summary { get; set; }
    public string Icon { get; set; }
}