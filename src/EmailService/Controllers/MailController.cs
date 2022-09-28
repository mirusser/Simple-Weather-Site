using System.Threading.Tasks;
using EmailService.Features.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly IMediator _mediator;

    public MailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SendEmail(SendEmailCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}