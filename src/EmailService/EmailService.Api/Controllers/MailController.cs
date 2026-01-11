using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using Common.Presentation.Controllers;
using EmailService.Application.Email.Commands;
using EmailService.Contracts.Email;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Api.Controllers;

public class MailController(
    IMediator mediator,
    IMapper mapper) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> SendEmail(
        SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<SendEmailCommand>(request);
        var result = await mediator.SendAsync(command, cancellationToken);

        return FromResult(result, mapper.Map<SendEmailResponse>);
    }
}