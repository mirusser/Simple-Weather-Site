using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Managers;
using WeatherSite.Models.WeatherPrediction;

namespace WeatherSite.Controllers;

public class WeatherPredictionController(WeatherForecastManager weatherForecastManager)
    : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetWeatherForecastPartial()
    {
        var vm = weatherForecastManager.GetGetWeatherForecastVm();

        return PartialView(vm);
    }

    //TODO: add proper validation
    [HttpPost]
    public async Task<IActionResult> GetWeatherForecastFromServicePartial(GetWeatherForecastVM weatherForecastVM)
    {
        var result = await weatherForecastManager
            .GetCurrentWeatherForCityByCityIdAsync(weatherForecastVM.CityId, HttpContext.RequestAborted);
            
        if (result.IsSuccess)
        {
            weatherForecastVM.CityName = Regex.Replace(weatherForecastVM.CityName, @"\t|\n|\r", "").TrimStart();
            weatherForecastVM.WeatherForecast = result.Value;
        }

        return PartialView(weatherForecastVM);
    }
}