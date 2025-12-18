using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries;

public class GetCityWeatherForecastByIdQuery : IRequest<CityWeatherForecastDocument?>
{
    public string Id { get; set; } = null!;
}

public class GetCityWeatherForecastByIdHandler(ICityWeatherForecastService cityWeatherForecastService) 
    : IRequestHandler<GetCityWeatherForecastByIdQuery, CityWeatherForecastDocument?>
{
	public async Task<CityWeatherForecastDocument?> Handle(
        GetCityWeatherForecastByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var cityWeatherForecast = await cityWeatherForecastService
            .GetAsync(request.Id, cancellationToken);

        return cityWeatherForecast;
    }
}