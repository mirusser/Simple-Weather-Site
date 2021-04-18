using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<WeatherForecastDto, CityWeatherForecastDocument>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.SearchDate, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summary))
                .ForPath(dest => dest.Temperature.TemperatureC, opt => opt.MapFrom(src => src.TemperatureC))
                .ForPath(dest => dest.Temperature.TemperatureF, opt => opt.MapFrom(src => src.TemperatureF));
        }
    }
}
