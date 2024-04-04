using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Entities;
using Mapster;

namespace CitiesGrpcService.Mappings;

public class CityReplyMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CityInfo, CityReply>()
            .Map(dest => dest.Id, src => (double)src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.State, src => src.State)
            .Map(dest => dest.Country, src => src.State)
            .Map(dest => dest.Coord.Lon, src => (double)src.Lon)
            .Map(dest => dest.Coord.Lat, src => (double)src.Lat);

        config.NewConfig<CityInfoPaginationDto, CitiesPaginationReply>();
    }
}