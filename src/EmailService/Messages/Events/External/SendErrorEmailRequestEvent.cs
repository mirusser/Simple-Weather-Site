using Convey.CQRS.Events;
using Convey.MessageBrokers;
using EmailService.Models;
using EmailService.Models.Dto;
using EmailService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmailService.Messages.Events.External
{
    [Message("weatherservice", "weatherhistoryservice")]
    public class SendErrorEmailRequestEvent : IEvent
    {
        public SendEmailAboutErrorDto SendEmailAboutErrorDto { get; set; }

        [JsonConstructor]
        public SendErrorEmailRequestEvent(SendEmailAboutErrorDto sendEmailAboutErrorDto)
        {
            SendEmailAboutErrorDto = sendEmailAboutErrorDto;
        }
    }

    public class SendErrorEmailRequestEventHandler : IEventHandler<SendErrorEmailRequestEvent>
    {
        private readonly IMailService _mailService;

        public SendErrorEmailRequestEventHandler(IMailService mailService)
        {
            _mailService = mailService;
        }

        //TODO: add proper error handling
        public async Task HandleAsync(SendErrorEmailRequestEvent @event)
        {
            if (@event != null && @event.SendEmailAboutErrorDto != null)
            {
                //TODO: add 'ToEmail' and 'Subject' value to settings or something
                var mailRequest = new MailRequest()
                {
                    ToEmail = "mirusser@gmail.com",
                    Subject = $"Exception from: {@event.SendEmailAboutErrorDto.AppName}",
                    Body = $"Exception message: {@event.SendEmailAboutErrorDto.Exception.Message}"
                };

                await _mailService.SendEmailAsync(mailRequest);
            }

            throw new NotImplementedException();
        }
    }
}
