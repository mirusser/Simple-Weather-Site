using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;

namespace WeatherSite.Controllers;

public class IconController(IconClient iconClient) : Controller
{
    [HttpPost]
    public async Task<byte[]?> Get(string icon)
    {
        var iconDto = await iconClient.GetIcon(icon);
        return iconDto?.FileContent;
    }
}