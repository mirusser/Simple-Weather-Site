using System;

namespace WeatherService.Models.Dto;

public class WeatherForecastDto
{
    public string City { get; set; }
    public string CountryCode { get; set; }
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Summary { get; set; }
    public string Icon { get; set; }
}