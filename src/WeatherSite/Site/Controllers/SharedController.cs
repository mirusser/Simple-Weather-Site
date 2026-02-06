using Microsoft.AspNetCore.Mvc;

namespace WeatherSite.Controllers;

public class SharedController : Controller
{
    [HttpPost]
    public IActionResult BootstrapLoader()
    {
        return PartialView("_BootstrapLoader");
    }
}