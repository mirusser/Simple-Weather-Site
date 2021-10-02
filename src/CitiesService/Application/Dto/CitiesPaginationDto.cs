using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class CitiesPaginationDto
    {
        public List<CityDto> Cities { get; set; } = new List<CityDto>();
        public int NumberOfAllCities { get; set; }
    }
}