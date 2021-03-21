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
using CitiesService.Models;

namespace CitiesService.Controllers
{
    [ApiController]
    [Route("api/city")]
    public class CityController : Controller
    {
        private readonly ICityManager _cityManager;

        public CityController(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        [HttpGet("{cityName}, {limit}", Name = "GetCitiesByName")]
        public async Task<ActionResult<List<City>>> GetCitiesByName(string cityName, int limit = 10)
        {
            var cities = await _cityManager.GetCitiesByName(cityName, limit);

            return cities != null && cities.Any() ? 
                Ok(cities) : 
                NotFound();
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
            return View();
        }
    }
}
