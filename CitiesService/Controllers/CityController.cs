using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using CitiesService.Helpers;
using Newtonsoft.Json;
using System.IO;

namespace CitiesService.Controllers
{
    public class Coord
    {
        public decimal lon { get; set; }
        public decimal lat { get; set; }
    }

    public class City
    {
        public decimal id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string Country { get; set; }
        public Coord coord { get; set; }
    }

    [ApiController]
    [Route("api/city")]
    public class CityController : Controller
    {
        private readonly HttpClient _httpClient;

        public CityController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var downloadFileUrl = "https://bulk.openweathermap.org/sample/city.list.json.gz";
            var filePath = "..\\CitiesService\\DownloadedCities\\city.list.json.gz";
            var decompressedFilePath = "..\\CitiesService\\DownloadedCities\\city.list.json";

            if (!System.IO.File.Exists(filePath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(downloadFileUrl, filePath);
                }
            }

            if (!System.IO.File.Exists(decompressedFilePath))
            {
                var fileInfo = new FileInfo(filePath);

                GzipHelper.Decompress(fileInfo);

                List<City> cities = new();

                using (StreamReader r = new(decompressedFilePath))
                {
                    string json = r.ReadToEnd();
                    cities = JsonConvert.DeserializeObject<List<City>>(json);
                }
            }


            return View();
        }
    }
}
