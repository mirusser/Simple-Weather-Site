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

        private readonly IQueryDispatcher _queryDispatcher;

        public WeatherForecastController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("{city}", Name = "GetByCityName")]
        public async Task<ActionResult<WeatherForecastDto>> GetByCityName([FromRoute] GetByCityNameQuery query)
        {
            var weatherForecastDto = await _queryDispatcher.QueryAsync(query);

            return weatherForecastDto != null ? Ok(weatherForecastDto) : NotFound();
        }

        [HttpGet("{city}", Name = "GetCityByNameFromXmlResponse")]
        public async Task<ActionResult<WeatherForecastDto>> GetCityByNameFromXmlResponse([FromRoute] GetByCityNameFromXmlResponseQuery query)
        {
            var weatherForecastDto = await _queryDispatcher.QueryAsync(query);

            return weatherForecastDto != null ? Ok(weatherForecastDto) : NotFound();
        }

        [HttpGet("{cityId}", Name = "GetByCityId")]
        public async Task<ActionResult<WeatherForecastDto>> GetByCityId([FromRoute] GetByCityIdQuery query)
        {
            var weatherForecastDto = await _queryDispatcher.QueryAsync(query);

            return weatherForecastDto != null ? Ok(weatherForecastDto) : NotFound();
        }
    }
}
