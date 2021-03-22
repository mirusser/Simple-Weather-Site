using CitiesService.Dto;
using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Queries
{
    public class GetCitiesQuery : IQuery<IEnumerable<CityDto>>
    {
        public string CityName { get; set; }
        public int Limit { get; set; }

        public GetCitiesQuery()
        {
            Limit = 10;
        }
    }
}
