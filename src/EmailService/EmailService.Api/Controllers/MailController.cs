using System.Threading.Tasks;
using Common.Presentation.Controllers;
using EmailService.Application.Email.Commands;
using EmailService.Contracts.Email;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Api.Controllers;

public class MailController(
	ISender mediator,
	IMapper mapper) : ApiController
{
	[HttpPost]
	public async Task<IActionResult> SendEmail(SendEmailRequest request)
	{
		var command = mapper.Map<SendEmailCommand>(request);
		var sendEmailResult = await mediator.Send(command);

		return sendEmailResult.Match(
			sendEmailResult => Ok(mapper.Map<SendEmailResponse>(sendEmailResult)),
			Problem);
	}
}