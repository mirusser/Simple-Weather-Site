using System.Threading.Tasks;
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
        public async Task<ActionResult<IconDto?>> GetIcon(GetIconQuery query)
        {
            return Ok(await _mediator.Send(query));
        }
    }
}