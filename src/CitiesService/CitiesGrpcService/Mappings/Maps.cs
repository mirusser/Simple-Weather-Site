using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dto;
using AutoMapper;
using Domain.Entities;

namespace CitiesGrpcService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<CityInfo, CityReply>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (double)src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.State))
                .ForPath(dest => dest.Coord.Lon, opt => opt.MapFrom(src => (double)src.Lon))
                .ForPath(dest => dest.Coord.Lat, opt => opt.MapFrom(src => (double)src.Lat));
        }
    }
}
