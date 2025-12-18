using System.Threading.Tasks;
using Common.Mediator;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Messages.Queries;
using WeatherService.Models.Dto;

namespace WeatherService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class WeatherForecastController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetByCityName(GetByCityNameQuery query)
    {
        return Ok(await mediator.SendAsync(query));
    }

    [HttpPost]
    public async Task<ActionResult<WeatherForecastDto>> GetCityByNameFromXmlResponse(GetByCityNameFromXmlResponseQuery query)
    {
        return Ok(await mediator.SendAsync(query));
    }

    [HttpPost]
    public async Task<ActionResult<WeatherForecastDto>> GetByCityId(GetByCityIdQuery query)
    {
        return Ok(await mediator.SendAsync(query));
    }
}