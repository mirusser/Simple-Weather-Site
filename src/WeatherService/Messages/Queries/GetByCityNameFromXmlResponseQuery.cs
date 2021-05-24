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
    public class GetByCityNameFromXmlResponseQuery : IQuery<WeatherForecastDto>
    {
        public string City { get; set; }
    }

    public class GetByCityNameFromXmlResponseHandler : IQueryHandler<GetByCityNameFromXmlResponseQuery, WeatherForecastDto>
    {
        private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetByCityNameFromXmlResponseHandler(
            WeatherClient weatherClient,
             IBusPublisher publisher)
        {
            _weatherClient = weatherClient;
            _publisher = publisher;
        }

        //TODO: publish message/event on successfuly getting forecast
        public async Task<WeatherForecastDto> HandleAsync(GetByCityNameFromXmlResponseQuery query)
        {
            var current = await _weatherClient.GetCurrentWeatherInXmlByCityNameAsync(query.City);

            if (current == null)
            {
                //TODO: logging
            }

            //TODO: add autoMapper
            WeatherForecastDto weatherForecastDto = new()
            {
                City = current.City.Name,
                CountryCode = current.City.Country,
                Summary = current.Weather.Value,
                TemperatureC = (int)current.Temperature.Value,
                Date = current.Lastupdate.Value
            };

            await _publisher.PublishAsync(new GotWeatherForecastEvent(weatherForecastDto));

            return weatherForecastDto;
        }
    }
}
