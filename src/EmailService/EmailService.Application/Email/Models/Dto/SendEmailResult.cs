namespace EmailService.Application.Email.Models.Dto;

public class SendEmailResult
{
    public string To { get; init; } = null!;
    public required string Subject { get; init; }
    public required string From { get; init; }
    public DateTimeOffset SendingDate { get; init; }
}