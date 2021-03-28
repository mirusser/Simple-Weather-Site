using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries.Handlers
{
    public class GetCityByNameFromXmlResponseHandler : IQueryHandler<GetCityByNameFromXmlResponseQuery, WeatherForecastDto>
    {
        //private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetCityByNameFromXmlResponseHandler(
            WeatherClient weatherClient)
        {
            _weatherClient = weatherClient;
            //_publisher = publisher;
        }

        //TODO: publish message/event on successfuly getting forecast
        public async Task<WeatherForecastDto> HandleAsync(GetCityByNameFromXmlResponseQuery query)
        {
            var current = await _weatherClient.GetCurrentWeatherInXmlByCityNameAsync(query.City);

            if (current == null)
            {
                //TODO: logging
            }

            //TODO: add autoMapper
            WeatherForecastDto weatherForecast = new()
            {
                City = current.City.Name,
                CountryCode = current.City.Country,
                Summary = current.Weather.Value,
                TemperatureC = (int)current.Temperature.Value,
                Date = current.Lastupdate.Value
            };

            return weatherForecast;
        }
    }
}
