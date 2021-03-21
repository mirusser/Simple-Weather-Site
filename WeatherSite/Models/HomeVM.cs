using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Models
{
    public class HomeVM
    {
        public string CityName { get; set; }

        [DisplayName("City name")]
        public decimal CityId { get; set; }
    }
}
