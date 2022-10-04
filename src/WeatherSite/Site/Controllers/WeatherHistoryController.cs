using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Clients;
using WeatherSite.Helpers;
using WeatherSite.Models.WeatherHistory;

namespace WeatherSite.Controllers;

public class WeatherHistoryController : Controller
{
    private readonly WeatherHistoryClient _weatherHistoryClient;

    public WeatherHistoryController(WeatherHistoryClient weatherHistoryClient)
    {
        _weatherHistoryClient = weatherHistoryClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeatherHistoryPagination()
    {
        WeatherHistoryVM vm = new();

        return View(vm);
    }

    //TODO: make a proper manager (business logic layer) and some more validation
    [HttpPost]
    public async Task<IActionResult> GetWeatherHistoryPaginationPartial(int pageNumber = 1, int numberOfEntitiesOnPage = 25)
    {
        var weatherHistoryForecastPagination = await _weatherHistoryClient.GetWeatherHistoryForecastPagination(pageNumber, numberOfEntitiesOnPage);
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