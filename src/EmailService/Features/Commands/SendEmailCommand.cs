using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EmailService.Services;
using MediatR;
using Microsoft.Extensions.Options;
using Models.Internal;
using Models.Settings;

namespace EmailService.Features.Commands
{
    public class SendEmailCommand : MailRequest, IRequest<Response<SentMailRequest>>
    {
    }

    public class SendEmailHandler : IRequestHandler<SendEmailCommand, Response<SentMailRequest>>
    {
        private readonly MailSettings _mailSettings;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;

        public SendEmailHandler(
            IOptions<MailSettings> options,
            IMailService mailService,
            IMapper mapper)
        {
            _mailSettings = options.Value;
            _mailService = mailService;
            _mapper = mapper;
        }

        //TODO: add some validation here? maybe? yes, no?
        public async Task<Response<SentMailRequest>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                return new Response<SentMailRequest>(message: "Request was null");

            request.To = !string.IsNullOrEmpty(request.To) ? request.To : _mailSettings.DefaultEmailReciever;
            var mailRequest = _mapper.Map<MailRequest>(request);

            var sentEmailRequest = await _mailService.SendEmailAsync(mailRequest);

            return new Response<SentMailRequest>(sentEmailRequest);
        }
    }
}