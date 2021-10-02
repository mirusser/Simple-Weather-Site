using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Models.Base;

namespace Models.Internal
{
    public class MailRequest : BaseMailRequest
    {
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}