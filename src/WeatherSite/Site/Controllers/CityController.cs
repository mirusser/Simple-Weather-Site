using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Helpers;
using WeatherSite.Logic.Managers;
using WeatherSite.Logic.Managers.Models.Records;
using WeatherSite.Models.City;
using WeatherSite.Models.City.Requests;

namespace WeatherSite.Controllers;

public class CityController(CityManager cityManager) : Controller
{
    [HttpGet]
    public IActionResult GetCitiesPagination()
    {
        CitiesPaginationVM vm = new();

        return View(vm);
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
    public async Task<List<City>> GetCitiesByName([FromBody] GetCitiesByNameRequest request)
    {
        var result = await cityManager.GetCitiesByName(request.CityName);
        return result;
    }
}