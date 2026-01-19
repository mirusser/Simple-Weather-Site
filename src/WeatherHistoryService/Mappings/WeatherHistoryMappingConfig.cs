using Mapster;
using MQModels.WeatherHistory;
using WeatherHistoryService.Features.Commands;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Mappings;

public class WeatherHistoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WeatherForecastDto, CityWeatherForecastDocument>()
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.CountryCode, src => src.CountryCode)
            .Map(dest => dest.SearchDate, src => src.Date)
            .Map(dest => dest.Summary, src => src.Summary)
            .Map(dest => dest.Temperature.TemperatureC, src => src.TemperatureC)
            .Map(dest => dest.Temperature.TemperatureF, src => src.TemperatureF);

        config.NewConfig<Temperature, TemperatureDto>().TwoWays();

        config.NewConfig<CreateWeatherForecastDocumentCommand, CityWeatherForecastDocument>();

        config.NewConfig<GotWeatherForecast, CityWeatherForecastDocument>()
            .Map(dest => dest.EventId, src => src.EventId)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.CountryCode, src => src.CountryCode)
            .Map(dest => dest.SearchDate, src => src.Date)
            .Map(dest => dest.Temperature.TemperatureC, src => src.TemperatureC)
            .Map(dest => dest.Temperature.TemperatureF, src => src.TemperatureF)
            .Map(dest => dest.Summary, src => src.Summary)
            .Map(dest => dest.Icon, src => src.Icon)
            .IgnoreNonMapped(true);
    }
}