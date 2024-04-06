using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace EmailService.Features.Models.Dto;

public class MailRequest
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? From { get; set; }

    public List<IFormFile>? Attachments { get; set; } = new List<IFormFile>();
}