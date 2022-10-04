using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherSite.Logic.Clients;

namespace WeatherSite.Controllers;

public class IconController : Controller
{
    private readonly IconClient _iconClient;

    public IconController(IconClient iconClient)
    {
        _iconClient = iconClient;
    }

    [HttpPost]
    public async Task<byte[]?> Get(string icon)
    {
        var iconDto = await _iconClient.GetIcon(icon);
        return iconDto?.FileContent;
    }
}