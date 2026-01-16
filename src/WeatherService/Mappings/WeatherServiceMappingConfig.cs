using System;
using Mapster;
using MQModels.WeatherHistory;
using WeatherService.Clients.Responses;
using WeatherService.Models;
using WeatherService.Models.Dto;

namespace WeatherService.Mappings;

public class WeatherServiceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Forecast, WeatherForecastDto>()
            .Map(dest => dest.City, src => src.name)
            .Map(dest => dest.CountryCode, src => src.sys != null ? src.sys.country : "")
            .Map(dest => dest.Summary,
                src => src.weather != null && src.weather.Length > 0 ? src.weather[0].description : "")
            .Map(dest => dest.TemperatureC, src => src.main != null ? (int)src.main.temp : 0)
            .Map(dest => dest.Icon, src => src.weather != null && src.weather.Length > 0 ? src.weather[0].icon : "")
            .Map(dest => dest.Date, src => DateTimeOffset.FromUnixTimeSeconds(src.dt).DateTime);

        config.NewConfig<Current, WeatherForecastDto>()
            .Map(dest => dest.City, src => src.City != null ? src.City.Name : "")
            .Map(dest => dest.CountryCode, src => src.City != null ? src.City.Country : "")
            .Map(dest => dest.Summary, src => src.Weather != null ? src.Weather.Value : "")
            .Map(dest => dest.TemperatureC, src => src.Temperature != null ? (int)src.Temperature.Value : 0)
            .Map(dest => dest.Icon, _ => "")
            .Map(dest => dest.Date, src => src.Lastupdate.Value);

        config.NewConfig<WeatherForecastDto, GotWeatherForecast>()
            .Map(dest => dest.EventId, src => Guid.NewGuid())
            .TwoWays();
    }
}