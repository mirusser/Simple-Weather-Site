using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries;

public class GetCityWeatherForecastsQuery : IRequest<IReadOnlyList<CityWeatherForecastDocument>>
{
}

public class GetCityWeatherForecastsHandler(ICityWeatherForecastService cityWeatherForecastService) 
    : IRequestHandler<GetCityWeatherForecastsQuery, IReadOnlyList<CityWeatherForecastDocument>>
{
	public async Task<IReadOnlyList<CityWeatherForecastDocument>> Handle(
        GetCityWeatherForecastsQuery request, 
        CancellationToken cancellationToken)
    {
        var cityWeatherForecastList = await cityWeatherForecastService
            .GetAllAsync(cancellationToken);

        return cityWeatherForecastList;
    }
}