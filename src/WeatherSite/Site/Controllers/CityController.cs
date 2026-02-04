using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;
using WeatherSite.Logic.Clients.Models.Records;
using WeatherSite.Logic.Helpers;
using WeatherSite.Models.City;

namespace WeatherSite.Controllers;

public class CityController(CityManager cityManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetCitiesPagination()
    {
        CitiesPaginationVM vm = new();

        return await Task.FromResult(View(vm));
    }

    [HttpPost]
    public async Task<IActionResult> GetCitiesPaginationPartial(int pageNumber = 1, int numberOfEntitiesOnPage = 25)
    {
        var url = Url.Action(
            nameof(CityController.GetCitiesPaginationPartial),
            MvcHelper.NameOfController<CityController>());

        var vm = await cityManager.GetCitiesPaginationPartialVMAsync(url, pageNumber, numberOfEntitiesOnPage);

        return PartialView(vm);
    }

    [HttpPost]
    public async Task<List<City>> GetCitiesByName([FromBody] Request request)
    {
        var result = await cityManager.GetCitiesByName(request.CityName);
        return result;
    }
}

public class Request
{
    public string CityName { get; set; }
    public int Limit { get; set; }
}