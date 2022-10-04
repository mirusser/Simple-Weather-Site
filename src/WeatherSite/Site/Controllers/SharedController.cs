using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WeatherSite.Controllers;

public class SharedController : Controller
{
    [HttpPost]
    public async Task<IActionResult> BootstrapLoader()
    {
        return PartialView("_BootstrapLoader");
    }
}