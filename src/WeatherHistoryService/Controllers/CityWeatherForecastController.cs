using Convey.CQRS.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Messages.Queries;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Controllers
{
    //TODO: refactor so all endpoints use mediatr
    [ApiController]
    [Route("api/[controller]")]
    public class CityWeatherForecastController : ControllerBase
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;
        private readonly IQueryDispatcher _queryDispatcher;

        public CityWeatherForecastController(
            ICityWeatherForecastService cityWeatherForecastService,
            IQueryDispatcher queryDispatcher)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
            _queryDispatcher = queryDispatcher;
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

        [HttpGet("{numberOfEntities}/{pageNumber}", Name = "GetCityWeatherForecastPagination")]
        public async Task<ActionResult<CityWeatherForecastPaginationDto>> GetCityWeatherForecastPagination([FromRoute] GetCityWeatherForecastPaginationQuery query)
        {
            var cityWeatherForecastPaginationDto = await _queryDispatcher.QueryAsync(query);

            return cityWeatherForecastPaginationDto != null ?
                Ok(cityWeatherForecastPaginationDto) :
                NoContent();
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