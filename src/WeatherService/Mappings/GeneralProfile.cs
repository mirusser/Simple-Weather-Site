using System;
using AutoMapper;
using MQModels.WeatherHistory;
using WeatherService.Clients;
using WeatherService.Models;
using WeatherService.Models.Dto;

namespace WeatherService.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<WeatherClient.Forecast, WeatherForecastDto>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.sys != null ? src.sys.country : ""))
                .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.weather != null && src.weather.Length > 0 ? src.weather[0].description : ""))
                .ForMember(dest => dest.TemperatureC, opt => opt.MapFrom(src => src.main != null ? (int)src.main.temp : 0))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.weather != null && src.weather.Length > 0 ? src.weather[0].icon : ""))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.dt).DateTime));

            CreateMap<Current, WeatherForecastDto>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City != null ? src.City.Name : ""))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.City != null ? src.City.Country : ""))
                .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Weather != null ? src.Weather.Value : ""))
                .ForMember(dest => dest.TemperatureC, opt => opt.MapFrom(src => src.Temperature != null ? (int)src.Temperature.Value : 0))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => ""))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Lastupdate.Value));

            CreateMap<WeatherForecastDto, IGotWeatherForecast>();
        }
    }
}