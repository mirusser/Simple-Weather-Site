using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherHistoryService.Models.Dto
{
    public class WeatherForecastDto
    {
        public string City { get; set; }
        public string CountryCode { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public string Summary { get; set; }
        public string Icon { get; set; }
    }
}