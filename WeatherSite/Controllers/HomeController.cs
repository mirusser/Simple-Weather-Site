using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

namespace WeatherSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WeatherForecastClient _weatherForecastClient;

        public HomeController(
            ILogger<HomeController> logger,
            WeatherForecastClient weatherForecastClient)
        {
            _logger = logger;
            _weatherForecastClient = weatherForecastClient;
        }

        public async Task<IActionResult> Index()
        {
            var weatherForecast = await _weatherForecastClient.GetCurrentWeatherForCity();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
