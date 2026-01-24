using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using Common.Presentation.Controllers;
using IconService.Application.Icon.Commands.Create;
using IconService.Application.Icon.Commands.Seed;
using IconService.Application.Icon.Models.Dto;
using IconService.Application.Icon.Queries.Get;
using IconService.Application.Icon.Queries.GetAll;
using IconService.Contracts.Icon;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace IconService.Api.Controllers;

public class IconsController(
    IMediator mediator,
    IMapper mapper) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateRequest request)
    {
        var command = mapper.Map<CreateCommand>(request);

        var createResult = await mediator.SendAsync(command);

        return Ok(mapper.Map<CreateResponse>(createResult));
    }

    [HttpPost]
    public async Task<IActionResult> Get(GetRequest request)
    {
        var query = mapper.Map<GetQuery>(request);

        var getResult = await mediator.SendAsync(query);

        return Ok(mapper.Map<GetResponse>(getResult));
    }

    [HttpPost]
    public async Task<IActionResult> GetAll(GetAllQuery query)
    {
        var getResult = await mediator.SendAsync(query);

        return Ok(mapper.Map<List<GetResult>>(getResult));
    }
    
    [HttpPost]
    public async Task<IActionResult> SeedAsync(SeedCommand request, CancellationToken ct)
    {
        // Safety guard – do NOT allow this in prod
        // if (!_env.IsDevelopment())
        //     return Forbid();

        var result = await mediator.SendAsync(request, ct);

        return Ok(new
        {
            message = "Icon collection seeded successfully",
            result.Upserted,
            result.Matched,
            result.Modified
        });
    }
}