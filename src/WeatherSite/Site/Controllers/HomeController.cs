using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Models;

namespace WeatherSite.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ErrorPartial(string code, string message)
    {
        var vm = new ErrorPartialVM()
        {
            Code = code,
            Message = message
        };

        return PartialView(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}