using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MQModels.Email
{
    public class SendEmail
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string? From { get; set; }
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}