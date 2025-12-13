using EmailService.Application.Email.Commands;
using EmailService.Contracts.Email;
using EmailService.Features.Models.Dto;
using Mapster;
using MimeKit;
using MQModels.Email;

namespace EmailService.Mappings;

public class EmailMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MimeMessage, SendEmailResult>()
            .Map(dest => dest.To, src => src.To.ToString())
            .Map(dest => dest.From, src => src.From.ToString())
            .Map(dest => dest.Body, src => src.Body.ToString())
            .Map(dest => dest.Subject, src => src.Subject)
            .Map(dest => dest.SendingDate, _ => DateTime.Now);

        config.NewConfig<SendEmail, SendEmailCommand>();
        config.NewConfig<SendEmailCommand, SendEmail>();
        config.NewConfig<SendEmailCommand, MailRequest>();
        config.NewConfig<SendEmailRequest, SendEmailCommand>();
        config.NewConfig<SendEmailCommand, SendEmailRequest>();
        config.NewConfig<SendEmailResult, SendEmailResponse>();
    }
}