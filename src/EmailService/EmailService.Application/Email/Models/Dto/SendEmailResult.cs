namespace EmailService.Application.Email.Models.Dto;

public class SendEmailResult
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string From { get; set; }
    public DateTimeOffset SendingDate { get; set; }
}