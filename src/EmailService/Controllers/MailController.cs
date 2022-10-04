using System.Threading.Tasks;
using Common.Presentation.Controllers;
using EmailService.Contracts.Email;
using EmailService.Features.Commands;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers;

public class MailController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper mapper;

    public MailController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        this.mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> SendEmail(SendEmailRequest request)
    {
        var command = mapper.Map<SendEmailCommand>(request);
        var sendEmailResult = await _mediator.Send(command);

        return sendEmailResult.Match(
            sendEmailResult => Ok(mapper.Map<SendEmailResponse>(sendEmailResult)),
            errors => base.Problem(errors));
    }
}