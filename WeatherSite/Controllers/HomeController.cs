using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WeatherSite.Clients;
using WeatherSite.Models;
using WeatherSite.Settings;

namespace WeatherSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WeatherForecastClient _weatherForecastClient;
        private readonly ApiEndpoints _apiEndpoints;

        public HomeController(
            ILogger<HomeController> logger,
            WeatherForecastClient weatherForecastClient,
            IOptions<ApiEndpoints> options)
        {
            _logger = logger;
            _weatherForecastClient = weatherForecastClient;
            _apiEndpoints = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new HomeVM()
            {
                CitiesServiceEndpoint = _apiEndpoints.CitiesServiceApiUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(HomeVM homeVM)
        {
            homeVM.WeatherForecast = await _weatherForecastClient.GetCurrentWeatherForCityByCityId(homeVM.CityId);
            return View(homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> ErrorPartial(string code, string message)
        {
            var vm = new ErrorPartialVM()
            {
                Code = code,
                Message = message
            };

            return PartialView(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
