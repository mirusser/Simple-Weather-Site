using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries;

public class GetCityWeatherForecastByIdQuery : IRequest<CityWeatherForecastDocument?>
{
    public string Id { get; set; } = null!;
}

public class GetCityWeatherForecastByIdHandler : IRequestHandler<GetCityWeatherForecastByIdQuery, CityWeatherForecastDocument?>
{
    private readonly ICityWeatherForecastService _cityWeatherForecastService;

    public GetCityWeatherForecastByIdHandler(ICityWeatherForecastService cityWeatherForecastService)
    {
        _cityWeatherForecastService = cityWeatherForecastService;
    }

    public async Task<CityWeatherForecastDocument?> Handle(GetCityWeatherForecastByIdQuery request, CancellationToken cancellationToken)
    {
        var cityWeatherForecast = await _cityWeatherForecastService.GetAsync(request.Id);

        return cityWeatherForecast;
    }
}