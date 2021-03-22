using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using CitiesService.Logic.Helpers;
using Newtonsoft.Json;
using CitiesService.Logic.Managers.Contracts;
using Microsoft.AspNetCore.Cors;
using CitiesService.Dto;
using Convey.CQRS.Queries;
using CitiesService.Queries;

namespace CitiesService.Controllers
{
    [ApiController]
    [Route("api/city")]
    [EnableCors("AllowAll")]
    public class CityController : ControllerBase
    {
        private readonly ICityManager _cityManager;
        private readonly IQueryDispatcher _queryDispatcher;

        public CityController(
            ICityManager cityManager,
            IQueryDispatcher queryDispatcher)
        {
            _cityManager = cityManager;
            _queryDispatcher = queryDispatcher;
        }

        //[HttpGet("{cityName}/{limit}", Name = "GetCitiesByName")]
        //public async Task<ActionResult<List<CityDto>>> GetCitiesByName(string cityName, int limit = 10)
        //{
        //    var cities = await _cityManager.GetCitiesByName(cityName, limit);

        //    return cities != null && cities.Any() ? 
        //        Ok(cities) : 
        //        NoContent();
        //}

        [HttpGet("{cityName}/{limit}", Name = "GetCitiesByName")]
        public async Task<ActionResult<List<CityDto>>> GetCitiesByName([FromRoute] GetCitiesQuery query)
        {
            var cities = await _queryDispatcher.QueryAsync(query);

            return cities != null && cities.Any() ?
                Ok(cities) :
                NoContent();
        }

        [HttpGet]
        [Route("DownloadCityFile")]
        public async Task<IActionResult> DownloadCityFile()
        {
            return NotFound();
        }

        [HttpGet]
        [Route("AddCityInfosToDatabase")]
        public async Task<IActionResult> AddCityInfosToDatabase()
        {
            var result = await _cityManager.SaveCitiesFromFileToDatabase();

            //TODO: return proper result depending on saving cities to database

            return Ok();
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            return NotFound();
        }
    }
}
