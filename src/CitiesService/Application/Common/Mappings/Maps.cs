using Application.Features.City.Commands.AddCitiesToDatabase;
using Application.Features.City.Models.Dto;
using Application.Features.City.Queries.GetCities;
using Application.Features.City.Queries.GetCitiesPagination;
using Application.Models.Dto;
using Contracts.City;
using Domain.Entities;
using Mapster;

namespace Application.Mappings;

public class CityMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetCityResult, CityInfo>()
            .Map(dest => dest.CityId, src => src.Id)
            .Map(dest => dest.CountryCode, src => src.Country)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.State, src => src.State)
            .Map(dest => dest.Lon, src => src.Coord != null ? src.Coord.Lon : default)
            .Map(dest => dest.Lat, src => src.Coord != null ? src.Coord.Lat : default)
            .IgnoreNonMapped(true);

        config.NewConfig<CityInfo, GetCityResult>()
            .Map(dest => dest.Id, src => src.CityId)
            .Map(dest => dest.Country, src => src.CountryCode)
            .Map(dest => dest.Coord.Lat, src => src.Lat)
            .Map(dest => dest.Coord.Lon, src => src.Lon);

        config.NewConfig<GetCitiesRequest, GetCitiesQuery>();
        config.NewConfig<GetCitiesPaginationRequest, GetCitiesPaginationQuery>();
        config.NewConfig<AddCitiesToDatabaseRequest, AddCitiesToDatabaseCommand>();

        config.NewConfig<GetCitiesResult, GetCitiesResponse>();
        config.NewConfig<GetCitiesPaginationResult, GetCitiesPaginationResponse>();
        config.NewConfig<AddCitiesToDatabaseResult, AddCitiesToDatabaseResponse>();
    }
}