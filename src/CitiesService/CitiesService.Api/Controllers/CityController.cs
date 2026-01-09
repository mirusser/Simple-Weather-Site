using System.Threading.Tasks;
using CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;
using CitiesService.Application.Features.City.Queries.GetCities;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Contracts.City;
using Common.Mediator;
using Common.Presentation.Controllers;
using MapsterMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CitiesService.Api.Controllers;

[EnableCors("AllowAll")]
public class CityController(
	IMediator mediator,
	IMapper mapper) : ApiController
{
	//TODO: add custom authorize attribute and/or authorization handler:
	// https://stackoverflow.com/questions/35609632/asp-net-5-authorize-against-two-or-more-policies-or-combined-policy
	[HttpPost]
	//[Authorize(Policy = "ApiScope")]
	public async Task<IActionResult> GetCitiesByName(GetCitiesRequest request)
	{
		var query = mapper.Map<GetCitiesQuery>(request);

		var getCitiesResult = await mediator.SendAsync(query);

		return Ok(mapper.Map<GetCitiesResponse>(getCitiesResult));
	}

	[HttpPost]
	public async Task<IActionResult> GetCitiesPagination(GetCitiesPaginationRequest request)
	{
		var query = mapper.Map<GetCitiesPaginationQuery>(request);

		var getCitiesPaginationResult = await mediator.SendAsync(query);

		return Ok(mapper.Map<GetCitiesPaginationResponse>(getCitiesPaginationResult));
	}

	[HttpPost]
	public async Task<IActionResult> AddCityInfoToDatabase(AddCitiesToDatabaseRequest request)
	{
		var command = mapper.Map<AddCitiesToDatabaseCommand>(request);

		var addCitiesToDatabaseResult = await mediator.SendAsync(command);

		return Ok(mapper.Map<AddCitiesToDatabaseResponse>(addCitiesToDatabaseResult));
	}
}