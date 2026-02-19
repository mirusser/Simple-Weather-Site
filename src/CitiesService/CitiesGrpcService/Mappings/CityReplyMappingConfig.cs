using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Entities;
using Mapster;

namespace CitiesGrpcService.Mappings;

public class CityReplyMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetCityResult, CityReply>()
            .Map(dest => dest.Id, src => (double)src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.State, src => src.State ?? string.Empty)
            .Map(dest => dest.Country, src => src.Country)
            .Map(dest => dest.Coord, src => src.Coord == null
                ? new Coord { Lon = 0, Lat = 0 }
                : new Coord { Lon = (double)src.Coord.Lon, Lat = (double)src.Coord.Lat });

        config.NewConfig<CityInfo, CityReply>()
            .Map(dest => dest.Id, src => (double)src.CityId)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.State, src => src.State ?? string.Empty)
            .Map(dest => dest.Country, src => src.CountryCode)
            .Map(dest => dest.Coord, src => new Coord { Lon = (double)src.Lon, Lat = (double)src.Lat });

        config.NewConfig<CityInfoPaginationDto, CitiesPaginationReply>();
    }
}
