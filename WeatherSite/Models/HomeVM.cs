using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static WeatherSite.Clients.WeatherForecastClient;

namespace WeatherSite.Models
{
    public class HomeVM
    {
        public string CityName { get; set; }

        [Required]
        [DisplayName("City name")]
        public decimal CityId { get; set; }

        public string CitiesServiceEndpoint { get; set; }

        public WeatherForecast WeatherForecast { get; set; }
    }
}
