using System.Threading.Tasks;
using Common.Mediator;
using Microsoft.AspNetCore.Mvc;
using WeatherHistoryService.Features.Commands;
using WeatherHistoryService.Features.Queries;

namespace WeatherHistoryService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class CityWeatherForecastController : ControllerBase
{
    private readonly IMediator _mediator;

    public CityWeatherForecastController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> GetAll(GetCityWeatherForecastsQuery query)
    {
        var cityWeatherForecastList = await _mediator.SendAsync(query);

        return cityWeatherForecastList != null ? Ok(cityWeatherForecastList) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Get(GetCityWeatherForecastByIdQuery query)
    {
        var cityWeatherForecast = await _mediator.SendAsync(query);

        return cityWeatherForecast != null ? Ok(cityWeatherForecast) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> GetCityWeatherForecastPagination(GetCityWeatherForecastPaginationQuery query)
    {
        return Ok(await _mediator.SendAsync(query));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWeatherForecastDocumentCommand command)
    {
        return Ok(await _mediator.SendAsync(command));
    }
}