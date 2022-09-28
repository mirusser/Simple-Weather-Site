using System.Threading.Tasks;
using Models.Internal;

namespace EmailService.Services;

public interface IMailService
{
    Task<SentMailRequest> SendEmailAsync(MailRequest request);
}