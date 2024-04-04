using System.Threading.Tasks;
using CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;
using CitiesService.Application.Features.City.Queries.GetCities;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using Common.Presentation.Controllers;
using CitiesService.Contracts.City;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CitiesService.Api.Controllers;

[EnableCors("AllowAll")]
public class CityController : ApiController
{
    private readonly IMediator mediator;
    private readonly IMapper mapper;

    public CityController(
        IMediator mediator,
        IMapper mapper)
    {
        this.mediator = mediator;
        this.mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> GetCitiesByName(GetCitiesRequest request)
    {
        var query = mapper.Map<GetCitiesQuery>(request);

        var getCitiesResult = await mediator.Send(query);

        return getCitiesResult.Match(
            getCitiesResult => Ok(mapper.Map<GetCitiesResponse>(getCitiesResult)),
            errors => base.Problem(errors));
    }

    [HttpPost]
    public async Task<IActionResult> GetCitiesPagination(GetCitiesPaginationRequest request)
    {
        var query = mapper.Map<GetCitiesPaginationQuery>(request);

        var getCitiesPaginationResult = await mediator.Send(query);

        return getCitiesPaginationResult.Match(
            getCitiesPaginationResult => Ok(mapper.Map<GetCitiesPaginationResponse>(getCitiesPaginationResult)),
            errors => base.Problem(errors));
    }

    [HttpPost]
    public async Task<IActionResult> AddCityInfosToDatabase(AddCitiesToDatabaseRequest request)
    {
        //var command = mapper.Map<AddCitiesToDatabaseCommand>(request);

        var command = new AddCitiesToDatabaseCommand()
        {
        };

        var addCitiesToDatabaseResult = await mediator.Send(command);

        return addCitiesToDatabaseResult.Match(
            addCitiesToDatabaseResult => Ok(mapper.Map<AddCitiesToDatabaseResponse>(addCitiesToDatabaseResult)),
            errors => base.Problem(errors));
    }
}