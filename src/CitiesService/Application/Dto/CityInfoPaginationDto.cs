using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Dto
{
    public class CityInfoPaginationDto
    {
        public List<CityInfo> CityInfos { get; set; } = new List<CityInfo>();
        public int NumberOfAllCities { get; set; }
    }
}