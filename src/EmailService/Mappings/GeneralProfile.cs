using System;
using AutoMapper;
using EmailService.Features.Commands;
using MimeKit;
using Models.Internal;
using MQModels.Email;

namespace EmailService.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<MimeMessage, SentMailRequest>()
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.To.ToString()))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.ToString()))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Body.ToString()))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.SendingDate, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<SendEmail, SendEmailCommand>().ReverseMap();
            CreateMap<SendEmailCommand, MailRequest>().ReverseMap();
        }
    }
}