using System.Threading.Tasks;
using EmailService.Features.Commands;
using MapsterMapper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using MQModels.Email;

namespace EmailService.Listeners;

public class SendEmailListener : IConsumer<SendEmail>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<SendEmailListener> _logger;

    public SendEmailListener(
        IMediator mediator,
        IMapper mapper,
        ILogger<SendEmailListener> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendEmail> context)
    {
        _logger.LogInformation("Got 'SendEmail' to consume");

        var sendEmailCommand = _mapper.Map<SendEmailCommand>(context.Message);

        //TODO: maybe do something with response here
        var response = await _mediator.Send(sendEmailCommand);
    }
}