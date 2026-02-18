namespace CitiesService.Domain.Entities.Dtos;

public record CityPatchDto(
    int Id,
    decimal? CityId,
    string? Name,
    string? State,
    string? CountryCode,
    decimal? Lon,
    decimal? Lat,
    byte[]? RowVersion);