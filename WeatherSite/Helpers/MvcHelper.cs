using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Helpers
{
    public class MvcHelper
    {
        public static string NameOfController<T>() where T : Controller
            => typeof(T).Name.Replace("Controller", string.Empty);

        public static string NameOfViewComponent<T>() where T : ViewComponent
            => typeof(T).Name.Replace("ViewComponent", string.Empty);
    }
}
