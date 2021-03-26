using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Clients;
using WeatherSite.Models.City;

namespace WeatherSite.Controllers
{
    public class CityController : Controller
    {
        private readonly CityClient _cityClient;

        public CityController(CityClient cityClient)
        {
            _cityClient = cityClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        //TODO: make a proper manager (business logic layer) and some more validation
        [HttpGet]
        public async Task<IActionResult> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25)
        {
            var citiesPagination = await _cityClient.GetCitiesPagination(pageNumber, numberOfCities);
            CitiesPaginationVM vm = new()
            {
                Cities = citiesPagination.Cities,
                PaginationVM = new()
                {
                    PageNumber = pageNumber,
                    NumberOfEntitiesOnPage = numberOfCities,
                    NumberOfPages = Convert.ToInt32(Math.Ceiling((decimal)citiesPagination.NumberOfAllCities / numberOfCities))
                }
            };

            return View(vm);
        }
    }
}
