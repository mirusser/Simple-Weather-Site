using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dto;
using AutoMapper;

namespace CitiesGrpcService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<CityDto, City>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (double)src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Coord.Lon, opt => opt.MapFrom(src => (double)src.Coord.Lon))
                .ForMember(dest => dest.Coord.Lat, opt => opt.MapFrom(src => (double)src.Coord.Lat));
        }
    }
}
