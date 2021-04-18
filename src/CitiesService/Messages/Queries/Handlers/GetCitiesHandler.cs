using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitiesService.Dto;
using CitiesService.Logic.Managers.Contracts;
using Convey.CQRS.Queries;

namespace CitiesService.Messages.Queries.Handlers
{
    public class GetCitiesHandler : IQueryHandler<GetCitiesQuery, IEnumerable<CityDto>>
    {
        private readonly ICityManager _cityManger;

        public GetCitiesHandler(ICityManager cityManger)
        {
            _cityManger = cityManger;
        }

        public async Task<IEnumerable<CityDto>> HandleAsync(GetCitiesQuery query)
        {
            var cities = _cityManger.GetCitiesByName(query.CityName, query.Limit);

            return cities;
        }
    }
}
