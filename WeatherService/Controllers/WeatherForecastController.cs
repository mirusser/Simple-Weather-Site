using Convey.CQRS.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Clients;
using WeatherService.Messages.Queries;
using WeatherService.Models.Dto;

namespace WeatherService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        //private readonly ILogger<WeatherForecastController> _logger;
        private readonly WeatherClient _weatherClient;

        private readonly IQueryDispatcher _queryDispatcher;

        public WeatherForecastController(
            WeatherClient weatherClient,
            IQueryDispatcher queryDispatcher)
        {
            _weatherClient = weatherClient;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("{city}", Name = "GetByCityName")]
        public async Task<ActionResult<WeatherForecastDto>> GetByCityName([FromRoute] GetCityByNameQuery query)
        {
            var weatherForecastDto = await _queryDispatcher.QueryAsync(query);

            return weatherForecastDto != null ? Ok(weatherForecastDto) : NotFound();
        }

        [HttpGet("{city}", Name = "GetCityByNameFromXmlResponse")]
        public async Task<WeatherForecastDto> GetCityByNameFromXmlResponse(string city)
        {
            var current = await _weatherClient.GetCurrentWeatherInXmlByCityNameAsync(city);

            WeatherForecastDto weatherForecast = new()
            {
                Summary = current.Weather.Value,
                TemperatureC = (int)current.Temperature.Value,
                Date = current.Lastupdate.Value
            };

            return weatherForecast;
        }

        [HttpGet("{cityId}", Name = "GetByCityId")]
        public async Task<ActionResult<WeatherForecastDto>> GetByCityId([FromRoute] GetCityByIdQuery query)
        {
            var weatherForecastDto = await _queryDispatcher.QueryAsync(query);

            return weatherForecastDto != null ? Ok(weatherForecastDto) : NotFound();
        }
    }
}
