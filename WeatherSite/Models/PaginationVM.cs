using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Models
{
    public class PaginationVM
    {
        public int PageNumber { get; set; }
        public int NumberOfEntitiesOnPage { get; set; }
        public int NumberOfPages { get; set; }
    }
}
