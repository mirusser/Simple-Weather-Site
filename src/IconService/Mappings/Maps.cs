using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Mongo.Documents;
using IconService.Models.Dto;

namespace IconService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<IconDto, IconDocument>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DayIcon, opt => opt.MapFrom(src => src.DayIcon));
        }
    }
}
