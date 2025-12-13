using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Presentation.Controllers;
using IconService.Application.Icon.Commands.Create;
using IconService.Application.Icon.Models.Dto;
using IconService.Application.Icon.Queries.Get;
using IconService.Application.Icon.Queries.GetAll;
using IconService.Contracts.Icon;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IconService.Api.Controllers;

public class IconsController(
    ISender mediator,
    IMapper mapper) : ApiController
{
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
        var getResult = await mediator.Send(query);
        
        return getResult.Match(
            getResult => Ok(mapper.Map<List<GetResult>>(getResult)),
            errors => base.Problem(errors));
    }
}