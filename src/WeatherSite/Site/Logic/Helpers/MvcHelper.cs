using Microsoft.AspNetCore.Mvc;

namespace WeatherSite.Logic.Helpers;

public static class MvcHelper
{
    public static string NameOfController<T>() where T : Controller
        => typeof(T).Name.Replace("Controller", string.Empty);

    public static string NameOfViewComponent<T>() where T : ViewComponent
        => typeof(T).Name.Replace("ViewComponent", string.Empty);
}