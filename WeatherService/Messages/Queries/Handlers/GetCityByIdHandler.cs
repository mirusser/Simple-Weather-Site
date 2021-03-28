using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries.Handlers
{
    public class GetCityByIdHandler : IQueryHandler<GetCityByIdQuery, WeatherForecastDto>
    {
        //private readonly IBusPublisher _publisher;
        private readonly WeatherClient _weatherClient;

        public GetCityByIdHandler(
            WeatherClient weatherClient)
        {
            _weatherClient = weatherClient;
            //_publisher = publisher;
        }

        //TODO: publish message/event on successfuly getting forecast
        public async Task<WeatherForecastDto> HandleAsync(GetCityByIdQuery query)
        {
            var forecast = await _weatherClient.GetCurrentWeatherByCityIdAsync(query.CityId);

            if (forecast == null)
            {
                //TODO: logging
            }

            //TODO: add automapper
            WeatherForecastDto weatherForecast = new()
            {
                Summary = forecast.weather[0].description,
                TemperatureC = (int)forecast.main.temp,
                Date = DateTimeOffset.FromUnixTimeSeconds(forecast.dt).DateTime
            };

            return weatherForecast;
        }
    }
}
