using CitiesService.Dto;
using CitiesService.Logic.Managers.Contracts;
using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Messages.Queries.Handlers
{
    public class GetCitiesPaginationHandler : IQueryHandler<GetCitiesPaginationQuery, CitiesPaginationDto>
    {
        private readonly ICityManager _cityManager;

        public GetCitiesPaginationHandler(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public async Task<CitiesPaginationDto> HandleAsync(GetCitiesPaginationQuery query)
        {
            var citiesPaginationDto = _cityManager.GetCitiesPagination(query.NumberOfCities, query.PageNumber);

            return citiesPaginationDto;
        }
    }
}
