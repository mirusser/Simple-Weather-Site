namespace CitiesService.Contracts.City;

public record GetCitiesPaginationRequest
(
    int NumberOfCities = 10,
    int PageNumber = 1
);