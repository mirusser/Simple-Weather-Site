using EmailService.Domain.Settings;
using EmailService.Features.Commands;
using MapsterMapper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQModels.Email;

namespace EmailService.Listeners;

public class SendEmailListener(
	IMediator mediator,
	IMapper mapper,
	IOptions<MailSettings> options,
	ILogger<SendEmailListener> logger) : IConsumer<SendEmail>
{
	public async Task Consume(ConsumeContext<SendEmail> context)
	{
		logger.LogInformation("Got {SendEmail} to consume", nameof(SendEmail));

		var sendEmailCommand = mapper.Map<SendEmailCommand>(context.Message);

		if (!string.IsNullOrWhiteSpace(sendEmailCommand.To))
		{
			sendEmailCommand.To = options.Value.DefaultEmailReciever;
		}

		if (!string.IsNullOrEmpty(sendEmailCommand.From))
		{
			sendEmailCommand.From = options.Value.From;
		}

		//TODO: maybe do something with response here
		var response = await mediator.Send(sendEmailCommand);
	}
}