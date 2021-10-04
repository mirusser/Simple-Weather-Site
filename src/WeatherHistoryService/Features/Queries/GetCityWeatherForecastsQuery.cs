using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries
{
    public class GetCityWeatherForecastsQuery : IRequest<IReadOnlyList<CityWeatherForecastDocument>>
    {
    }

    public class GetCityWeatherForecastsHandler : IRequestHandler<GetCityWeatherForecastsQuery, IReadOnlyList<CityWeatherForecastDocument>>
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;
        private readonly IMapper _mapper;

        public GetCityWeatherForecastsHandler(
            ICityWeatherForecastService cityWeatherForecastService,
            IMapper mapper)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CityWeatherForecastDocument>> Handle(GetCityWeatherForecastsQuery request, CancellationToken cancellationToken)
        {
            var cityWeatherForecastList = await _cityWeatherForecastService.GetAll();

            return cityWeatherForecastList;
        }
    }
}