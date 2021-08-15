using Application.Dto;
using Application.Interfaces.Managers;
using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Queries
{
    public class GetCitiesPaginationQuery : IQuery<CitiesPaginationDto>
    {
        public int NumberOfCities { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetCitiesPaginationHandler : IQueryHandler<GetCitiesPaginationQuery, CitiesPaginationDto>
    {
        private readonly ICityManager _cityManager;

        public GetCitiesPaginationHandler(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public async Task<CitiesPaginationDto> HandleAsync(GetCitiesPaginationQuery query)
        {
            var citiesPaginationDto = await _cityManager.GetCitiesPagination(query.NumberOfCities, query.PageNumber);

            return citiesPaginationDto;
        }
    }
}
