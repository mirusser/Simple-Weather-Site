using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Logic.Clients;
using WeatherSite.Settings;

namespace WeatherSite.Controllers
{
    public class IconController : Controller
    {
        private readonly IconClient _iconClient;

        public IconController(IconClient iconClient)
        {
            _iconClient = iconClient;
        }

        [HttpPost]
        public async Task<byte[]> Get(string description, bool dayIcon)
        {
            var icon = await _iconClient.GetIcon(description, dayIcon);
            return icon.FileContent;
        }
    }
}
