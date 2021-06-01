using Convey.CQRS.Events;
using Convey.MessageBrokers;
using EmailService.Models;
using EmailService.Models.Dto;
using EmailService.Services;
using EmailService.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmailService.Messages.Events.External
{
    [Message("serviceexchange")]
    public class SendEmailRequestEvent : IEvent
    {
        public SendEmailDto SendEmailDto { get; set; }

        [JsonConstructor]
        public SendEmailRequestEvent(SendEmailDto sendEmailAboutErrorDto)
        {
            SendEmailDto = sendEmailAboutErrorDto;
        }
    }

    public class SendEmailRequestEventHandler : IEventHandler<SendEmailRequestEvent>
    {
        private readonly IMailService _mailService;
        private readonly MailSettings _mailSettings;

        public SendEmailRequestEventHandler(
            IMailService mailService,
            IOptions<MailSettings> options)
        {
            _mailService = mailService;
            _mailSettings = options.Value;
        }

        //TODO: add proper error handling
        public async Task HandleAsync(SendEmailRequestEvent @event)
        {
            if (@event != null && @event.SendEmailDto != null)
            {
                //TODO: add automapper
                var mailRequest = new MailRequest()
                {
                    ToEmail = 
                        !string.IsNullOrEmpty(@event.SendEmailDto.ToEmail) ? 
                        @event.SendEmailDto.ToEmail :
                        _mailSettings.DefaultEmailReciever,
                    Subject = @event.SendEmailDto.Subject,
                    Body = @event.SendEmailDto.Body,
                    Attachments = @event.SendEmailDto.Attachments
                };

                await _mailService.SendEmailAsync(mailRequest);
            }

            await Task.CompletedTask;
        }
    }
}
