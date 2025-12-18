using Common.Mediator;
using EmailService.Application.Email.Commands;
using EmailService.Domain.Settings;
using MapsterMapper;
using MassTransit;
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
			sendEmailCommand.To = options.Value.DefaultEmailReceiver;
		}

		if (!string.IsNullOrEmpty(sendEmailCommand.From))
		{
			sendEmailCommand.From = options.Value.From;
		}

		//TODO: maybe do something with response here
		var response = await mediator.SendAsync(sendEmailCommand, context.CancellationToken);
	}
}