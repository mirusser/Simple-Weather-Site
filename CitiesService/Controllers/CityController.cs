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

namespace CitiesService.Controllers
{
    [ApiController]
    [Route("api/city")]
    public class CityController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ICityManager _cityManager;

        public CityController(
            HttpClient httpClient,
            ICityManager cityManager)
        {
            _httpClient = httpClient;
            _cityManager = cityManager;
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
