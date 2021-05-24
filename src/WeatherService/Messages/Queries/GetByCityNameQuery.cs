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
    public class GetByCityNameQuery : IQuery<WeatherForecastDto>
    {
        public string City { get; set; }
    }

    public class GetByCityNameHandler : IQueryHandler<GetByCityNameQuery, WeatherForecastDto>
    {
        private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetByCityNameHandler(
            WeatherClient weatherClient,
            IBusPublisher publisher)
        {
            _weatherClient = weatherClient;
            _publisher = publisher;
        }

        //TODO: publish message/event on successfuly getting forecast
        public async Task<WeatherForecastDto> HandleAsync(GetByCityNameQuery query)
        {
            var forecast = await _weatherClient.GetCurrentWeatherByCityNameAsync(query.City);

            if (forecast == null)
            {
                //TODO: logging
            }

            //TODO: add autoMapper
            WeatherForecastDto weatherForecastDto = new()
            {
                City = forecast.name,
                CountryCode = forecast.sys.country,
                Summary = forecast.weather[0].description,
                TemperatureC = (int)forecast.main.temp,
                Date = DateTimeOffset.FromUnixTimeSeconds(forecast.dt).DateTime
            };

            await _publisher.PublishAsync(new GotWeatherForecastEvent(weatherForecastDto));

            return weatherForecastDto;
        }
    }
}
