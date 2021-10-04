using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries
{
    public class GetCityWeatherForecastPaginationQuery : IRequest<CityWeatherForecastPaginationDto>
    {
        public int NumberOfEntities { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetCityWeatherForecastPaginationHandler : IRequestHandler<GetCityWeatherForecastPaginationQuery, CityWeatherForecastPaginationDto>
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;

        public GetCityWeatherForecastPaginationHandler(ICityWeatherForecastService cityWeatherForecastService)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
        }

        public async Task<CityWeatherForecastPaginationDto> Handle(GetCityWeatherForecastPaginationQuery request, CancellationToken cancellationToken)
        {
            var cityWeatherForecastPaginationDto = await _cityWeatherForecastService.GetCityWeatherForecastPagination(request.NumberOfEntities, request.PageNumber);

            return cityWeatherForecastPaginationDto;
        }
    }
}