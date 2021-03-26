using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherSite.Clients.Models.Records;

namespace WeatherSite.Models.City
{
    public class CitiesPaginationVM
    {
        public PaginationVM PaginationVM { get; set; }

        //TODO: make a proper view model of this property
        public List<Clients.Models.Records.City> Cities { get; set; }
    }
}
