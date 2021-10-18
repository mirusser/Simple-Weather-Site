using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Mongo.Documents;
using IconService.Models.Dto;
using IconService.Messages.Commands;

namespace IconService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<GetIconDto, IconDocument>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.DayIcon, opt => opt.MapFrom(src => src.DayIcon))
                .ForMember(dest => dest.FileContent, opt => opt.MapFrom(src => src.FileContent))
                .ReverseMap();

            CreateMap<IconDocument, CreateIconDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.DayIcon, opt => opt.MapFrom(src => src.DayIcon))
                .ForMember(dest => dest.FileContent, opt => opt.MapFrom(src => src.FileContent))
                .ReverseMap();

            CreateMap<IEnumerable<IconDocument>, IEnumerable<CreateIconDto>>();

            CreateMap<CreateIconCommand, IconDocument>();
        }
    }
}
