using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Dto
{
    public class CitiesPaginationDto
    {
        public List<CityDto> Cities { get; set; }
        public int NumberOfAllCities { get; set; }
    }
}
