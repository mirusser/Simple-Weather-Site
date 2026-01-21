using System.Threading;
using System.Threading.Tasks;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using WeatherHistoryService.Features.Commands;
using WeatherHistoryService.Features.Queries;

namespace WeatherHistoryService.Controllers;

public class CityWeatherForecastController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> GetAll(
        GetCityWeatherForecastsQuery query,
        CancellationToken ct)
    {
        var cityWeatherForecastList = await Mediator.SendAsync(query, ct);

        return cityWeatherForecastList != null ? Ok(cityWeatherForecastList) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Get(
        GetCityWeatherForecastByIdQuery query,
        CancellationToken ct)
    {
        var cityWeatherForecast = await Mediator.SendAsync(query, ct);

        return cityWeatherForecast != null ? Ok(cityWeatherForecast) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> GetCityWeatherForecastPagination(
        GetCityWeatherForecastPaginationQuery query,
        CancellationToken ct)
    {
        return Ok(await Mediator.SendAsync(query, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateWeatherForecastDocumentCommand command,
        CancellationToken ct)
    {
        return Ok(await Mediator.SendAsync(command, ct));
    }
}