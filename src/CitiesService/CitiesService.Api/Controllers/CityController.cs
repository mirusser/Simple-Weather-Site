using System.Threading.Tasks;
using CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;
using CitiesService.Application.Features.City.Queries.GetCities;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Contracts.City;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CitiesService.Api.Controllers;

[EnableCors("AllowAll")]
public class CityController : ApiController
{
    //TODO: add custom authorize attribute and/or authorization handler:
    // https://stackoverflow.com/questions/35609632/asp-net-5-authorize-against-two-or-more-policies-or-combined-policy
    [HttpPost]
    //[Authorize(Policy = "ApiScope")]
    public async Task<IActionResult> GetCitiesByName(GetCitiesRequest request)
    {
        var query = Mapper.Map<GetCitiesQuery>(request);

        var result = await Mediator.SendAsync(query);

        return FromResult(result, Mapper.Map<GetCitiesResponse>);
    }

    [HttpPost]
    public async Task<IActionResult> GetCitiesPagination(GetCitiesPaginationRequest request)
    {
        var query = Mapper.Map<GetCitiesPaginationQuery>(request);

        var result = await Mediator.SendAsync(query);

        return FromResult(result, Mapper.Map<GetCitiesPaginationResponse>);
    }

    [HttpPost]
    public async Task<IActionResult> AddCityInfoToDatabase(AddCitiesToDatabaseRequest request)
    {
        var command = Mapper.Map<AddCitiesToDatabaseCommand>(request);

        var result = await Mediator.SendAsync(command);

        return FromResult(result, Mapper.Map<AddCitiesToDatabaseResponse>);
    }
}