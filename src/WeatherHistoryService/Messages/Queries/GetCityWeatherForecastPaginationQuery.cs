using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Messages.Queries
{
    public class GetCityWeatherForecastPaginationQuery : IQuery<CityWeatherForecastPaginationDto>
    {
        public int NumberOfEntities { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetCityWeatherForecastPaginationHandler : IQueryHandler<GetCityWeatherForecastPaginationQuery, CityWeatherForecastPaginationDto>
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;

        public GetCityWeatherForecastPaginationHandler(ICityWeatherForecastService cityWeatherForecastService)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
        }

        public async Task<CityWeatherForecastPaginationDto> HandleAsync(GetCityWeatherForecastPaginationQuery query)
        {
            var cityWeatherForecastPaginationDto = await _cityWeatherForecastService.GetCityWeatherForecastPagination(query.NumberOfEntities, query.PageNumber);

            return cityWeatherForecastPaginationDto;
        }
    }
}
