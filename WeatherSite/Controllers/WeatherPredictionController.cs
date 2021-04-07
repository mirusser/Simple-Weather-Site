using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Clients;
using WeatherSite.Models.WeatherPrediction;
using WeatherSite.Settings;

namespace WeatherSite.Controllers
{
    public class WeatherPredictionController : Controller
    {
        private readonly WeatherForecastClient _weatherForecastClient;
        private readonly ApiEndpoints _apiEndpoints;

        public WeatherPredictionController(
            WeatherForecastClient weatherForecastClient,
            IOptions<ApiEndpoints> options)
        {
            _weatherForecastClient = weatherForecastClient;
            _apiEndpoints = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        //TODO: add proper validation and move to another business logic layer (manager/service)
        [HttpGet]
        public async Task<IActionResult> GetWeatherForecastPartial()
        {
            var vm = new GetWeatherForecastVM()
            {
                CitiesServiceEndpoint = _apiEndpoints.CitiesServiceApiUrl
            };

            return PartialView(vm);
        }

        //TODO: add proper validation and move to another business logic layer (manager/service)
        [HttpPost]
        public async Task<IActionResult> GetWeatherForecastFromServicePartial(GetWeatherForecastVM weatherForecastVM)
        {
            weatherForecastVM.WeatherForecast = await _weatherForecastClient.GetCurrentWeatherForCityByCityId(weatherForecastVM.CityId);

            return PartialView(weatherForecastVM);
        }

    }
}
