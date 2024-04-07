using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries;

public class GetCityWeatherForecastsQuery : IRequest<IReadOnlyList<CityWeatherForecastDocument>>
{
}

public class GetCityWeatherForecastsHandler : IRequestHandler<GetCityWeatherForecastsQuery, IReadOnlyList<CityWeatherForecastDocument>>
{
    private readonly ICityWeatherForecastService _cityWeatherForecastService;

    public GetCityWeatherForecastsHandler(ICityWeatherForecastService cityWeatherForecastService)
    {
        _cityWeatherForecastService = cityWeatherForecastService;
    }

    public async Task<IReadOnlyList<CityWeatherForecastDocument>> Handle(GetCityWeatherForecastsQuery request, CancellationToken cancellationToken)
    {
        var cityWeatherForecastList = await _cityWeatherForecastService.GetAll();

        return cityWeatherForecastList;
    }
}