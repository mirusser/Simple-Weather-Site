namespace EmailService.Contracts.Email;

public record SendEmailResponse
(
    string To,
    string Subject,
    string From,
    DateTimeOffset SendingDate
);