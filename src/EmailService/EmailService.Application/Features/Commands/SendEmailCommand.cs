using Common.Mediator;
using Common.Presentation.Http;
using EmailService.Application.Features.Models.Dto;
using EmailService.Domain.Common.Errors;
using EmailService.Domain.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.Application.Features.Commands;

public class SendEmailCommand
    : MailRequest, IRequest<Result<SendEmailResult>>
{
}

public class SendEmailHandler(
    IOptionsSnapshot<MailSettings> options,
    TimeProvider timeProvider,
    ILogger<SendEmailHandler> logger)
    : IRequestHandler<SendEmailCommand, Result<SendEmailResult>>
{
    private readonly MailSettings mailSettings = options.Value;

    public async Task<Result<SendEmailResult>> Handle(
        SendEmailCommand request,
        CancellationToken cancellationToken)
    {
        var to = string.IsNullOrWhiteSpace(request.To) ? mailSettings.DefaultEmailReceiver : request.To;
        var from = string.IsNullOrWhiteSpace(request.From) ? mailSettings.From : request.From;

        MailRequest mailRequest = new()
        {
            To = to,
            From = from,
            Subject = request.Subject,
            Body = request.Body,
            Attachments = request.Attachments
        };

        using var cts = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        cts.CancelAfter(TimeSpan.FromSeconds(60));

        SendEmailResult sentEmailResult;

        try
        {
            sentEmailResult = await SendEmailAsync(mailRequest, cancellationToken);
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            var message = "Mail server authentication failed."; 
            logger.LogError(ex,message);
            
            return Result<SendEmailResult>.Fail(new Problem(
                ErrorCodes.MailAuthenticationFailed,
                message,
                StatusCodes.Status503ServiceUnavailable));
        }
        catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxUnavailable)
        {
            var message = "Recipient mailbox is unavailable."; 
            logger.LogError(ex,message);
            
            return Result<SendEmailResult>.Fail(new Problem(
                ErrorCodes.MailInvalidRecipient,
                message,
                StatusCodes.Status400BadRequest));
        }
        catch (SmtpCommandException ex)
        {
            var message = "Mail server rejected the request."; 
            logger.LogError(ex,message);
            
            return Result<SendEmailResult>.Fail(new Problem(
                ErrorCodes.MailProtocolError,
                message,
                StatusCodes.Status503ServiceUnavailable));
        }
        catch (SmtpProtocolException ex)
        {
            var message = "Mail protocol error."; 
            logger.LogError(ex,message);
            
            return Result<SendEmailResult>.Fail(new Problem(
                ErrorCodes.MailProtocolError,
                message,
                StatusCodes.Status503ServiceUnavailable));
        }
        catch (IOException ex)
        {
            var message = "Mail server is unreachable."; 
            logger.LogError(ex,message);
            
            return Result<SendEmailResult>.Fail(new Problem(
                ErrorCodes.MailServerUnavailable,
                message,
                StatusCodes.Status503ServiceUnavailable));
        }

        return Result<SendEmailResult>.Ok(sentEmailResult);
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
            SendingDate = timeProvider.GetUtcNow()
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

        if (request.Attachments is null)
        {
            return builder;
        }

        foreach (var file in request.Attachments.Where(f => f.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            await builder.Attachments.AddAsync(
                file.FileName,
                stream,
                ContentType.Parse(file.ContentType),
                cancellationToken);
        }

        return builder;
    }

    private async Task SendEmailAsync(
        MimeMessage email,
        CancellationToken cancellationToken)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(mailSettings.From, mailSettings.Password, cancellationToken);
        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}