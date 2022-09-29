using EmailService.Domain.Settings;
using EmailService.Features.Models.Dto;
using ErrorOr;
using MailKit.Net.Smtp;
using MailKit.Security;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.Features.Commands;

public class SendEmailCommand
    : MailRequest, IRequest<ErrorOr<SendEmailResult>>
{
}

public class SendEmailHandler
    : IRequestHandler<SendEmailCommand, ErrorOr<SendEmailResult>>
{
    private readonly MailSettings _mailSettings;
    private readonly IMapper _mapper;

    public SendEmailHandler(
        IOptions<MailSettings> options,
        IMapper mapper)
    {
        _mailSettings = options.Value;
        _mapper = mapper;
    }

    public async Task<ErrorOr<SendEmailResult>> Handle(
        SendEmailCommand request,
        CancellationToken cancellationToken)
    {
        request.To = !string.IsNullOrEmpty(request.To)
            ? request.To
            : _mailSettings.DefaultEmailReciever;
        request.From = !string.IsNullOrEmpty(request.From)
            ? request.From
            : _mailSettings.From;
        var mailRequest = _mapper.Map<MailRequest>(request);

        var sentEmailRequest = await SendEmailAsync(mailRequest);

        return sentEmailRequest;
    }

    private async Task<SendEmailResult> SendEmailAsync(MailRequest request)
    {
        var mimeMessage = await CreateMimeMessage(request);
        await SendEmailAsync(mimeMessage);

        var sentEmailRequest = _mapper.Map<SendEmailResult>(mimeMessage);

        return sentEmailRequest;
    }

    private async Task<MimeMessage> CreateMimeMessage(MailRequest mailRequest)
    {
        var email = new MimeMessage()
        {
            Sender = MailboxAddress.Parse(mailRequest.From ?? _mailSettings.From),
            Subject = mailRequest.Subject
        };

        email.To.Add(MailboxAddress.Parse(mailRequest.To));
        email.From.Add(new MailboxAddress(_mailSettings.DisplayName, mailRequest.From ?? _mailSettings.From));

        var builder = CreateBodyBuilder(mailRequest);
        email.Body = builder.ToMessageBody();

        return email;
    }

    private static BodyBuilder CreateBodyBuilder(MailRequest request)
    {
        var builder = new BodyBuilder
        {
            HtmlBody = request.Body
        };

        if (request.Attachments?.Any() != true)
            return builder;

        byte[] fileBytes;

        foreach (var file in request.Attachments)
        {
            if (file.Length > 0)
            {
                using var ms = new MemoryStream();
                file.CopyTo(ms);
                fileBytes = ms.ToArray();

                builder.Attachments.Add(file.Name, fileBytes, ContentType.Parse(file.ContentType));
            }
        }

        return builder;
    }

    private async Task SendEmailAsync(MimeMessage email)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.From, _mailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}