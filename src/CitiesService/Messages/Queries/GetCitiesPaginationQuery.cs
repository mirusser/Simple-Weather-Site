using CitiesService.Dto;
using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Messages.Queries
{
    public class GetCitiesPaginationQuery : IQuery<CitiesPaginationDto>
    {
        public int NumberOfCities { get; set; }
        public int PageNumber { get; set; }
    }
}
