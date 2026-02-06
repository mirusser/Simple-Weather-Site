using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;
using WeatherSite.Logic.Helpers;
using WeatherSite.Models.WeatherHistory;

namespace WeatherSite.Controllers;

public class WeatherHistoryController(WeatherHistoryManager weatherHistoryManager)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetWeatherHistoryPagination()
    {
        WeatherHistoryVM vm = weatherHistoryManager.GetWeatherHistoryVm();

        return View(vm);
    }

    //TODO: add some more validation
    [HttpPost]
    public async Task<IActionResult> GetWeatherHistoryPaginationPartial(
        int pageNumber = 1,
        int numberOfEntitiesOnPage = 25)
    {
        var url = Url.Action(
            nameof(WeatherHistoryController.GetWeatherHistoryPaginationPartial),
            MvcHelper.NameOfController<WeatherHistoryController>());

        var vm = await weatherHistoryManager.GetWeatherHistoryPaginationVmAsync(
            url,
            pageNumber,
            numberOfEntitiesOnPage,
            HttpContext.RequestAborted);

        return PartialView(vm);
    }
}