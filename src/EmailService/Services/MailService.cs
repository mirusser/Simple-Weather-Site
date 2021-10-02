using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Models.Internal;
using Models.Settings;

namespace EmailService.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IMapper _mapper;

        public MailService(
            IOptions<MailSettings> mailSettings,
            IMapper mapper)
        {
            _mailSettings = mailSettings.Value;
            _mapper = mapper;
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

            if (request.Attachments == null || !request.Attachments.Any())
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

        public async Task<SentMailRequest> SendEmailAsync(MailRequest request)
        {
            var mimeMessage = await CreateMimeMessage(request);
            await SendEmailAsync(mimeMessage);

            var sentEmailRequest = _mapper.Map<SentMailRequest>(mimeMessage);

            return sentEmailRequest;
        }
    }
}