using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherSite.Clients;
using WeatherSite.Logic.Clients;
using WeatherSite.Models.WeatherPrediction;
using WeatherSite.Settings;

namespace WeatherSite.Controllers;

public class WeatherPredictionController : Controller
{
    private readonly ApiEndpoints _apiEndpoints;

    private readonly WeatherForecastClient _weatherForecastClient;
    private readonly CityClient _cityClient;

    public WeatherPredictionController(
        WeatherForecastClient weatherForecastClient,
        CityClient cityClient,
        IconClient iconClient,
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
            CitiesServiceEndpoint = _apiEndpoints.CitiesServiceApiUrl
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

        return PartialView(weatherForecastVM);
    }
}