using CitiesService.Application.Common.Mappings;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Features.City.Queries.GetCities;
using CitiesService.Contracts.City;
using CitiesService.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace CitiesService.Tests.Mappings;

public class CitiesServiceMappingConfigTests
{
    private static IMapper CreateMapper()
    {
        var config = new TypeAdapterConfig();
        new CitiesServiceMappingConfig().Register(config);
        return new Mapper(config);
    }

    [Fact]
    public void Maps_GetCitiesRequest_To_GetCitiesQuery()
    {
        var mapper = CreateMapper();

        var request = new GetCitiesRequest(CityName: "Lon", Limit: 7);
        var query = mapper.Map<GetCitiesQuery>(request);

        Assert.Equal("Lon", query.CityName);
        Assert.Equal(7, query.Limit);
    }

    [Fact]
    public void Maps_CityInfo_To_GetCityResult()
    {
        var mapper = CreateMapper();

        var entity = new CityInfo
        {
            Id = 1,
            CityId = 999m,
            Name = "London",
            CountryCode = "GB",
            Lat = 1.23m,
            Lon = 4.56m,
            State = "England"
        };

        var dto = mapper.Map<GetCityResult>(entity);

        Assert.Equal(999m, dto.Id);
        Assert.Equal("London", dto.Name);
        Assert.Equal("GB", dto.Country);
        Assert.Equal("England", dto.State);
        Assert.NotNull(dto.Coord);
        Assert.Equal(1.23m, dto.Coord!.Lat);
        Assert.Equal(4.56m, dto.Coord.Lon);
    }

    [Fact]
    public void Maps_GetCitiesResult_To_GetCitiesResponse()
    {
        var mapper = CreateMapper();

        var result = new GetCitiesResult
        {
            Cities =
            [
                new GetCityResult
                {
                    Id = 1,
                    Name = "A",
                    Country = "X",
                    State = null,
                    Coord = new Coord { Lat = 1, Lon = 2 }
                }
            ]
        };

        var response = mapper.Map<GetCitiesResponse>(result);

        Assert.Single(response.Cities);
    }

    [Fact]
    public void Maps_GetCitiesPaginationResult_To_GetCitiesPaginationResponse()
    {
        var mapper = CreateMapper();

        var result = new GetCitiesPaginationResult
        {
            NumberOfAllCities = 10,
            Cities =
            [
                new GetCityResult { Id = 1, Name = "A", Country = "X", Coord = new Coord { Lat = 1, Lon = 2 } }
            ]
        };

        var response = mapper.Map<GetCitiesPaginationResponse>(result);

        Assert.Equal(10, response.NumberOfAllCities);
        Assert.Single(response.Cities);
    }
}
