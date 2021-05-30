using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Models.Dto
{
    public class SendEmailAboutErrorDto
    {
        public string AppName { get; set; }
        public Exception Exception { get; set; }
    }
}
