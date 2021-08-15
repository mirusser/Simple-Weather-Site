using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Models.City
{
    public class CitiesPaginationPartialVM
    {
        public PaginationVM PaginationVM { get; set; }

        //TODO: make a proper view model of this property
        public List<Clients.Models.Records.City> Cities { get; set; }

        public string Url { get; set; }
    }
}
