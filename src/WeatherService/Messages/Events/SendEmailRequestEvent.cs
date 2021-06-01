using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Events
{
    public class SendEmailRequestEvent
    {
        public SendEmailDto SendEmailDto { get; set; }

        public SendEmailRequestEvent(SendEmailDto sendEmailDto)
        {
            SendEmailDto = sendEmailDto;
        }
    }
}
