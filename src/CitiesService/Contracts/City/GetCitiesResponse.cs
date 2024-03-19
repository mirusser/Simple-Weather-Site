namespace Contracts.City;

public record GetCitiesResponse(IEnumerable<GetCityResponse> Cities);

public record CoordResponse
(
    decimal Lon,
    decimal Lat
);

public record GetCityResponse
(
    decimal Id,
    string? Name,
    string? State,
    string? Country,
    CoordResponse? Coord
);