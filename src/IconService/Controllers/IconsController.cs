using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Controllers
{
    public class IconsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
