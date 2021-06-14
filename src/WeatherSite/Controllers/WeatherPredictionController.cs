using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeatherSite.Clients;
using WeatherSite.Models.WeatherPrediction;
using WeatherSite.Settings;

namespace WeatherSite.Controllers
{
    public class WeatherPredictionController : Controller
    {
        private readonly ApiEndpoints _apiEndpoints;

        private readonly WeatherForecastClient _weatherForecastClient;
        private readonly CityClient _cityClient;

        public WeatherPredictionController(
            WeatherForecastClient weatherForecastClient,
            CityClient cityClient,
            IOptions<ApiEndpoints> options)
        {
            _weatherForecastClient = weatherForecastClient;
            _cityClient = cityClient;
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
                CitiesServiceEndpoint = _apiEndpoints.CitiesServiceApiUrl,
                CitiesServiceLocalEndpoint = _apiEndpoints.CitiesServiceLocalApiUrl
            };

            return PartialView(vm);
        }

        //TODO: add proper validation and move to another business logic layer (manager/service)
        [HttpPost]
        public async Task<IActionResult> GetWeatherForecastFromServicePartial(GetWeatherForecastVM weatherForecastVM)
        {
            if (weatherForecastVM != null && weatherForecastVM.CityId != default)
            {
                weatherForecastVM.CityName = Regex.Replace(weatherForecastVM.CityName, @"\t|\n|\r", "").TrimStart();
                weatherForecastVM.WeatherForecast = await _weatherForecastClient.GetCurrentWeatherForCityByCityId(weatherForecastVM.CityId);
            }

            //_cityClient.get

            return PartialView(weatherForecastVM);
        }

    }
}
