using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Messages.Queries;
using WeatherService.Models.Dto;

namespace WeatherService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherForecastController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> GetByCityName(GetByCityNameQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<WeatherForecastDto>> GetCityByNameFromXmlResponse(GetByCityNameFromXmlResponseQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<WeatherForecastDto>> GetByCityId(GetByCityIdQuery query)
    {
        return Ok(await _mediator.Send(query));
    }
}