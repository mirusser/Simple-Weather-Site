namespace Contracts.City;

public record GetCitiesPaginationResponse
(
    List<GetCityResponse> Cities,
    int NumberOfAllCities
);