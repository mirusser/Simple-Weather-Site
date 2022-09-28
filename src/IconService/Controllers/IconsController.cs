using System.Threading.Tasks;
using Common.Presentation.Controllers;
using IconService.Application.Icon.Commands.Create;
using IconService.Application.Icon.Queries.Get;
using IconService.Application.Icon.Queries.GetAll;
using IconService.Contracts.Icon;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IconService.Controllers;

public class IconsController : ApiController
{
    private readonly ISender mediator;
    private readonly IMapper mapper;

    public IconsController(
        ISender mediator,
        IMapper mapper)
    {
        this.mediator = mediator;
        this.mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequest request)
    {
        var command = mapper.Map<CreateCommand>(request);

        var createResult = await mediator.Send(command);

        return createResult.Match(
            createResult => Ok(mapper.Map<CreateResponse>(createResult)),
            errors => base.Problem(errors));
    }

    [HttpPost]
    public async Task<IActionResult> Get(GetRequest request)
    {
        var query = mapper.Map<GetQuery>(request);

        var getResult = await mediator.Send(query);

        return getResult.Match(
            getResult => Ok(mapper.Map<GetResponse>(getResult)),
            errors => base.Problem(errors));
    }

    [HttpPost]
    public async Task<IActionResult> GetAll(GetAllQuery query)
    {
        return Ok(await mediator.Send(query));
    }
}