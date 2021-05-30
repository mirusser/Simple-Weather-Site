using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Events
{
    public class SendErrorEmailRequestEvent
    {
        public SendEmailAboutErrorDto SendEmailAboutErrorDto { get; set; }

        public SendErrorEmailRequestEvent(SendEmailAboutErrorDto sendEmailAboutErrorDto)
        {
            SendEmailAboutErrorDto = sendEmailAboutErrorDto;
        }
    }
}
