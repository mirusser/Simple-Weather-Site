using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;

namespace WeatherSite.Controllers;

public class IconController(IconClient iconClient) : Controller
{
    [HttpPost]
    public async Task<byte[]> Get(string icon)
    {
        var result = await iconClient.GetIconAsync(icon, HttpContext.RequestAborted);

        return result.IsSuccess ? result.Value.FileContent : null;
    }
}