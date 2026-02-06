using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;

namespace WeatherSite.Controllers;

public class IconController(IconManager iconManager) : Controller
{
    [HttpPost]
    public async Task<byte[]> Get(string icon)
    {
        var result = await iconManager.GetIconAsync(icon, HttpContext.RequestAborted);

        return result.IsSuccess ? result.Value.FileContent : null;
    }
}