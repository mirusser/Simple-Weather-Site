using Microsoft.AspNetCore.Http;

namespace EmailService.Application.Features.Models.Dto;

public class MailRequest
{
    public string To { get; set; } = null!;
    public required string Subject { get; init; } = null!;
    public required string Body { get; init; } = null!;
    public string? From { get; set; }
    public List<IFormFile>? Attachments { get; init; } = [];
}