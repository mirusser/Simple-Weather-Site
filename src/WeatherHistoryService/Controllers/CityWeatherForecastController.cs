using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Controllers
{
    [Route("api/[controller]")]
    public class CityWeatherForecastController : ControllerBase
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;

        public CityWeatherForecastController(ICityWeatherForecastService cityWeatherForecastService)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cityWeatherForecastList = await _cityWeatherForecastService.GetAll();

            return cityWeatherForecastList != null ? Ok(cityWeatherForecastList) : NotFound();
        }

        [HttpGet("{id:length(24)}", Name = "Get")]
        public async Task<IActionResult> Get(string id)
        {
            var cityWeatherForecast = await _cityWeatherForecastService.GetAsync(id);

            return cityWeatherForecast != null ? Ok(cityWeatherForecast) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<CityWeatherForecastDocument>> Create([FromBody] CityWeatherForecastDocument cityWeatherForecastIn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var cityWeatherForecast = await _cityWeatherForecastService.CreateAsync(cityWeatherForecastIn);

            return CreatedAtRoute(nameof(Get), new { id = cityWeatherForecast.Id }, cityWeatherForecast);
        }
    }
}
