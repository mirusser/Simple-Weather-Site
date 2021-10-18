using System.Threading.Tasks;
using IconService.Messages.Commands;
using IconService.Messages.Queries;
using IconService.Models.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IconService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IconsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IconsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIconCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost]
        public async Task<ActionResult<GetIconDto?>> Get(GetIconQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<IActionResult> GetAll(GetAllIconsQuery query)
        {
            return Ok(await _mediator.Send(query));
        }
    }
}