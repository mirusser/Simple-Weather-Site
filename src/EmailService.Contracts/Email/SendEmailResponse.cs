namespace EmailService.Contracts.Email;

public record SendEmailResponse
(
    string To,
    string Subject,
    string Body,
    string From,
    DateTime SendingDate
);