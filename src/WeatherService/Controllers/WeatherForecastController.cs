using System.Threading;
using System.Threading.Tasks;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Messages.Queries;
using WeatherService.Models.Contracts;
using WeatherService.Models.Dto;

namespace WeatherService.Controllers;

public class WeatherForecastController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> GetByCityName(
        GetByCityNameRequest request,
        CancellationToken cancellationToken)
    {
        GetByCityNameQuery query = new () { City = request.Name };
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetCityByNameFromXmlResponse(
        GetCityByNameFromXmlResponseRequest request,
        CancellationToken cancellationToken)
    {
        GetByCityNameFromXmlResponseQuery query = new() {City =  request.City};
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetByCityId(
        GetByCityIdRequest request,
        CancellationToken cancellationToken)
    {
        GetByCityIdQuery query = new() { CityId =  request.CityId };
        var result = await Mediator.SendAsync(query, cancellationToken);

        return FromResult(result);
    }
}