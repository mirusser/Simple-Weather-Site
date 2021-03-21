﻿using AutoMapper;
using CitiesService.Data;
using CitiesService.Data.DatabaseModels;
using CitiesService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<City, CityInfo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Lon, opt => opt.MapFrom(src => src.Coord.Lon))
                .ForMember(dest => dest.Lat, opt => opt.MapFrom(src => src.Coord.Lat));
        }
    }
}
