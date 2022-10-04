using Microsoft.AspNetCore.Http;

namespace EmailService.Contracts.Email;

public record SendEmailRequest
(
    string To,
    string Subject,
    string Body,
    string? From,
    List<IFormFile>? Attachments
);