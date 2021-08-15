using CitiesGrpcService;
using GrpcCitiesClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Clients;
using WeatherSite.Helpers;
using WeatherSite.Models.City;

namespace WeatherSite.Controllers
{
    public class CityController : Controller
    {
        private readonly CityClient _cityClient;
        private readonly ICitiesClient _citiesClient;

        public CityController(CityClient cityClient, ICitiesClient citiesClient)
        {
            _cityClient = cityClient;
            _citiesClient = citiesClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitiesPagination()
        {
            CitiesPaginationVM vm = new();

            return View(vm);
        }

        //TODO: make a proper manager (business logic layer) and some more validation
        [HttpPost]
        public async Task<IActionResult> GetCitiesPaginationPartial(int pageNumber = 1, int numberOfEntitiesOnPage = 25)
        {
            var citiesPagination = await _cityClient.GetCitiesPagination(pageNumber, numberOfEntitiesOnPage);

            //var foo = new List<City>();

            //await foreach (var cityReply in _citiesClient.GetCitiesStream(pageNumber, numberOfEntitiesOnPage))
            //{
            //    foo.Add(cityReply);
            //}

            CitiesPaginationPartialVM vm = new()
            {
                Cities = citiesPagination?.Cities,
                PaginationVM = new()
                {
                    ElementId = "#cities-pagination-partial-div",
                    Url = Url.Action(nameof(CityController.GetCitiesPaginationPartial), MvcHelper.NameOfController<CityController>()),
                    PageNumber = pageNumber,
                    NumberOfEntitiesOnPage = numberOfEntitiesOnPage,
                    NumberOfPages = Convert.ToInt32(Math.Ceiling((decimal)citiesPagination?.NumberOfAllCities / numberOfEntitiesOnPage))
                }
            };

            return PartialView(vm);
        }
    }
}
