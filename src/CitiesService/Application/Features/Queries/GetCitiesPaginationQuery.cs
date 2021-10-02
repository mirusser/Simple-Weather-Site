using System.Threading;
using System.Threading.Tasks;
using Application.Dto;
using Application.Interfaces.Managers;
using MediatR;

namespace Application.Features.Queries
{
    public class GetCitiesPaginationQuery : IRequest<CitiesPaginationDto>
    {
        public int NumberOfCities { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetCitiesPaginationHandler : IRequestHandler<GetCitiesPaginationQuery, CitiesPaginationDto>
    {
        private readonly ICityManager _cityManager;

        public GetCitiesPaginationHandler(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public async Task<CitiesPaginationDto> Handle(GetCitiesPaginationQuery request, CancellationToken cancellationToken)
        {
            var citiesPaginationDto = await _cityManager.GetCitiesPaginationDto(request.NumberOfCities, request.PageNumber);

            return citiesPaginationDto ?? new();
        }
    }
}