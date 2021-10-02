using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class Coord
    {
        public decimal Lon { get; set; }
        public decimal Lat { get; set; }
    }

    public class CityDto
    {
        public decimal Id { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public Coord? Coord { get; set; }
    }
}