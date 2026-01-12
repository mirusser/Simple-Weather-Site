using System.Threading;
using System.Threading.Tasks;
using Common.Presentation.Controllers;
using EmailService.Application.Email.Commands;
using EmailService.Application.Email.Models.Dto;
using EmailService.Contracts.Email;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Api.Controllers;

public class MailController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> SendEmail(
        SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = Mapper.Map<SendEmailCommand>(request);
        var result = await Mediator.SendAsync(command, cancellationToken);

        return FromResult<SendEmailResult, SendEmailResponse>(result);
    }
}