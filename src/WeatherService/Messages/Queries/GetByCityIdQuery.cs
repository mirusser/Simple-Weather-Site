using Convey.CQRS.Queries;
using Convey.MessageBrokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Clients;
using WeatherService.Messages.Events;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries
{
    public class GetByCityIdQuery : IQuery<WeatherForecastDto>
    {
        public decimal CityId { get; set; }
    }

    public class GetByCityIdHandler : IQueryHandler<GetByCityIdQuery, WeatherForecastDto>
    {
        private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetByCityIdHandler(
            WeatherClient weatherClient,
            IBusPublisher publisher)
        {
            _weatherClient = weatherClient;
            _publisher = publisher;
        }

        public async Task<WeatherForecastDto> HandleAsync(GetByCityIdQuery query)
        {
            var forecast = await _weatherClient.GetCurrentWeatherByCityIdAsync(query.CityId);
            WeatherForecastDto weatherForecastDto = null;

            if (forecast == null)
            {
                //TODO: logging
            }
            else
            {
                //TODO: add automapper
                weatherForecastDto = new()
                {
                    City = forecast.name,
                    CountryCode = forecast.sys.country,
                    Summary = forecast.weather[0].description,
                    TemperatureC = (int)forecast.main.temp,
                    Date = DateTimeOffset.FromUnixTimeSeconds(forecast.dt).DateTime,
                    Icon = forecast.weather[0].icon
                };

                await _publisher.PublishAsync(new GotWeatherForecastEvent(weatherForecastDto));
            }

            return weatherForecastDto;
        }
    }
}