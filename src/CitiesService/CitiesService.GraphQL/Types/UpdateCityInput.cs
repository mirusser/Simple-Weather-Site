using CitiesService.Domain.Entities;

namespace CitiesService.GraphQL.Types;

public record UpdateCityInput(
    int Id,
    decimal CityId,
    string Name,
    string? State,
    string CountryCode,
    decimal Lon,
    decimal Lat,
    string? RowVersion
);

public record UpdateCityPayload(CityInfo City);