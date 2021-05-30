using EmailService.Models;
using EmailService.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        private async Task<MimeMessage> CreateMimeMessage(MailRequest mailRequest)
        {
            var email = new MimeMessage()
            {
                Sender = MailboxAddress.Parse(_mailSettings.Mail),
                Subject = mailRequest.Subject
            };

            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));

            var builder = new BodyBuilder();

            if (mailRequest.Attachments != null && mailRequest.Attachments.Any())
            {
                byte[] fileBytes;

                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }

                        builder.Attachments.Add(file.Name, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();

            return email;
        }

        private async Task SendEmailAsync(MimeMessage email)
        {
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }

        public async Task SendEmailAsync(MailRequest request)
        {
            var mimeMessage = await CreateMimeMessage(request);
            await SendEmailAsync(mimeMessage);
        }
    }
}
