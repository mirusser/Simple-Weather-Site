using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Controllers
{
    public class SharedController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> BootstrapLoader()
        {
            return PartialView("_BootstrapLoader");
        }
    }
}
