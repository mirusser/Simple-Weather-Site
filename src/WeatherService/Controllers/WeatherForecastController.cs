using System.Threading;
using System.Threading.Tasks;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Features.Queries;

namespace WeatherService.Controllers;

public class WeatherForecastController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> GetByCityName(
        GetByCityNameQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetCityByNameFromXmlResponse(
        GetByCityNameFromXmlResponseQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetByCityId(
        GetByCityIdQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }
}