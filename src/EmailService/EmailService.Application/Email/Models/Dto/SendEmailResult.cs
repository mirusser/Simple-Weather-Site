using System;

namespace EmailService.Features.Models.Dto;

public class SendEmailResult
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string From { get; set; }
    public DateTime SendingDate { get; set; }
}