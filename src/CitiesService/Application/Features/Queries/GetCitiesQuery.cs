using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Dto;
using Application.Interfaces.Managers;
using MediatR;

namespace Application.Features.Queries
{
    public class GetCitiesQuery : IRequest<IEnumerable<CityDto>>
    {
        public string CityName { get; set; } = null!;
        public int Limit { get; set; } = 10;
    }

    public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, IEnumerable<CityDto>>
    {
        private readonly ICityManager _cityManger;

        public GetCitiesHandler(ICityManager cityManger)
        {
            _cityManger = cityManger;
        }

        public async Task<IEnumerable<CityDto>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CityName) ||
                string.IsNullOrEmpty(request.CityName.TrimStart()) ||
                string.IsNullOrEmpty(request.CityName.TrimEnd()))
                return new List<CityDto>();

            request.CityName = request.CityName.TrimStart().TrimEnd();

            var cities = await _cityManger.GetCitiesByName(request.CityName, request.Limit);

            return cities ?? new();
        }
    }
}