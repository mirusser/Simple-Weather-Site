using Application.Dto;
using Application.Interfaces.Managers;
using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Queries
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

    public class GetCitiesHandler : IQueryHandler<GetCitiesQuery, IEnumerable<CityDto>>
    {
        private readonly ICityManager _cityManger;

        public GetCitiesHandler(ICityManager cityManger)
        {
            _cityManger = cityManger;
        }

        public async Task<IEnumerable<CityDto>> HandleAsync(GetCitiesQuery query)
        {
            var cities = await _cityManger.GetCitiesByName(query.CityName, query.Limit);

            return cities;
        }
    }
}
