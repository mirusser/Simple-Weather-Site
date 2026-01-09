using Common.Mediator;
using EmailService.Domain.Settings;
using EmailService.Features.Models.Dto;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.Application.Email.Commands;

public class SendEmailCommand
    : MailRequest, IRequest<SendEmailResult>
{
}

public class SendEmailHandler(
    IOptions<MailSettings> options
) : IRequestHandler<SendEmailCommand, SendEmailResult>
{
    private readonly MailSettings mailSettings = options.Value;

    public async Task<SendEmailResult> Handle(
        SendEmailCommand request,
        CancellationToken cancellationToken)
    {
        request.To = !string.IsNullOrEmpty(request.To)
            ? request.To
            : mailSettings.DefaultEmailReceiver;
        request.From = !string.IsNullOrEmpty(request.From)
            ? request.From
            : mailSettings.From;

        MailRequest mailRequest = new()
        {
            To = request.To,
            From = request.From,
            Subject = request.Subject,
            Body = request.Body,
            Attachments = request.Attachments
        };

        var sentEmailResult = await SendEmailAsync(mailRequest, cancellationToken);

        return sentEmailResult;
    }

    private async Task<SendEmailResult> SendEmailAsync(
        MailRequest request,
        CancellationToken cancellationToken = default)
    {
        var mimeMessage = await CreateMimeMessage(request, cancellationToken);
        await SendEmailAsync(mimeMessage, cancellationToken);

        SendEmailResult result = new()
        {
            To = mimeMessage.To.ToString(),
            From = mimeMessage.From.ToString(),
            Subject = mimeMessage.Subject,
            Body = mimeMessage.Body.ToString(),
            SendingDate = DateTime.Now
        };

        return result;
    }

    private async Task<MimeMessage> CreateMimeMessage(
        MailRequest mailRequest,
        CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage()
        {
            Sender = MailboxAddress.Parse(mailRequest.From ?? mailSettings.From),
            Subject = mailRequest.Subject
        };

        email.To.Add(MailboxAddress.Parse(mailRequest.To));
        email.From.Add(new MailboxAddress(mailSettings.DisplayName, mailRequest.From ?? mailSettings.From));

        var builder = await CreateBodyBuilderAsync(mailRequest, cancellationToken);
        email.Body = builder.ToMessageBody();

        return email;
    }

    private async Task<BodyBuilder> CreateBodyBuilderAsync(
        MailRequest request,
        CancellationToken cancellationToken = default)
    {
        var builder = new BodyBuilder
        {
            HtmlBody = request.Body
        };

        if (request.Attachments?.Any() != true)
            return builder;

        foreach (var file in request.Attachments.Where(file => file.Length > 0))
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);

            await builder.Attachments.AddAsync(
                file.Name,
                ms,
                ContentType.Parse(file.ContentType),
                cancellationToken);
        }

        return builder;
    }

    private async Task SendEmailAsync(
        MimeMessage email,
        CancellationToken cancellationToken = default)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(mailSettings.From, mailSettings.Password, cancellationToken);
        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}