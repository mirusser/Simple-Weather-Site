using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using MQModels.Email;

namespace EmailService.Listeners
{
    public class SendEmailListener : IConsumer<SendEmail>
    {
        private readonly ILogger<SendEmailListener> _logger;

        public SendEmailListener(ILogger<SendEmailListener> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendEmail> context)
        {
            _logger.LogInformation("Got 'SendEmail' to consume");
        }
    }
}