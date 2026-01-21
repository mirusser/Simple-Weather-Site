using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherSite.Logic.Clients;
using WeatherSite.Logic.Settings;
using WeatherSite.Models.WeatherPrediction;

namespace WeatherSite.Controllers;

public class WeatherPredictionController(
    WeatherForecastClient weatherForecastClient,
    IOptions<ApiEndpoints> options)
    : Controller
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

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
            CitiesServiceEndpoint = apiEndpoints.CitiesServiceApiUrl
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
            weatherForecastVM.WeatherForecast = await weatherForecastClient.GetCurrentWeatherForCityByCityId(weatherForecastVM.CityId);
        }

        return PartialView(weatherForecastVM);
    }
}