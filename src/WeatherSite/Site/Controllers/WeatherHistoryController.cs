using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherSite.Helpers;
using WeatherSite.Logic.Clients;
using WeatherSite.Models.WeatherHistory;
using WeatherSite.Settings;

namespace WeatherSite.Controllers;

public class WeatherHistoryController : Controller
{
    private readonly WeatherHistoryClient weatherHistoryClient;
    private readonly ApiEndpoints apiEndpoints;

    public WeatherHistoryController(
        WeatherHistoryClient weatherHistoryClient,
        IOptions<ApiEndpoints> options)
    {
        this.weatherHistoryClient = weatherHistoryClient;
        apiEndpoints = options.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeatherHistoryPagination()
    {
        WeatherHistoryVM vm = new()
        {
            SignalRServerUrl = apiEndpoints.SignalRServer
        };

        return View(vm);
    }

    //TODO: make a proper manager (business logic layer) and some more validation
    [HttpPost]
    public async Task<IActionResult> GetWeatherHistoryPaginationPartial(int pageNumber = 1, int numberOfEntitiesOnPage = 25)
    {
        var weatherHistoryForecastPagination = await weatherHistoryClient.GetWeatherHistoryForecastPagination(pageNumber, numberOfEntitiesOnPage);
        WeatherHistoryPaginationPartialVM vm = new()
        {
            CityWeatherForecastDocuments = weatherHistoryForecastPagination?.WeatherForecastDocuments,
            PaginationVM = new()
            {
                ElementId = "#weather-history-pagination-partial-div",
                Url = Url.Action(nameof(WeatherHistoryController.GetWeatherHistoryPaginationPartial), MvcHelper.NameOfController<WeatherHistoryController>()),
                PageNumber = pageNumber,
                NumberOfEntitiesOnPage = numberOfEntitiesOnPage,
                NumberOfPages = Convert.ToInt32(Math.Ceiling((decimal)weatherHistoryForecastPagination?.NumberOfAllEntities / numberOfEntitiesOnPage))
            }
        };

        return PartialView(vm);
    }
}