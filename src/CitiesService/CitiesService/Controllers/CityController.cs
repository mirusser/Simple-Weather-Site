using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dto;
using Application.Features.Commands;
using Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CitiesService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [EnableCors("AllowAll")]
    public class CityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<List<CityDto>>> GetCitiesByName(GetCitiesQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult<CitiesPaginationDto>> GetCitiesPagination(GetCitiesPaginationQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<IActionResult> AddCityInfosToDatabase(AddCitiesToDatabaseCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}