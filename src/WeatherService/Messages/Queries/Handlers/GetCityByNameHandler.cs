using Convey.CQRS.Queries;
using Convey.MessageBrokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries.Handlers
{
    public class GetCityByNameHandler : IQueryHandler<GetCityByNameQuery, WeatherForecastDto>
    {
        //private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetCityByNameHandler(
            WeatherClient weatherClient)
        {
            _weatherClient = weatherClient;
            //_publisher = publisher;
        }

        //TODO: publish message/event on successfuly getting forecast
        public async Task<WeatherForecastDto> HandleAsync(GetCityByNameQuery query)
        {
            var forecast = await _weatherClient.GetCurrentWeatherByCityNameAsync(query.City);

            if (forecast == null)
            {
                //TODO: logging
            }

            //TODO: add autoMapper
            WeatherForecastDto weatherForecast = new()
            {
                City = forecast.name,
                CountryCode = forecast.sys.country,
                Summary = forecast.weather[0].description,
                TemperatureC = (int)forecast.main.temp,
                Date = DateTimeOffset.FromUnixTimeSeconds(forecast.dt).DateTime
            };

            return weatherForecast;
        }
    }
}
