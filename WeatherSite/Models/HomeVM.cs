using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Clients.Models.Records;

namespace WeatherSite.Models
{
    //TODO: move it to proper location (directory)
    public class HomeVM
    {
        public string CityName { get; set; }

        [Required]
        [DisplayName("City name")]
        public decimal CityId { get; set; }

        public string CitiesServiceEndpoint { get; set; }

        //TODO: make a proper view model of this property
        public WeatherForecast WeatherForecast { get; set; }
    }
}
