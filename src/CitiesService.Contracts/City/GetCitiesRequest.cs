namespace CitiesService.Contracts.City;

public record GetCitiesRequest
(
    string? CityName,
    int Limit = 10
);