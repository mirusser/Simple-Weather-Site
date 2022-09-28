namespace CitiesService.Contracts.City;

public record GetCitiesPaginationRequest
(
    int NumberOfCities,
    int PageNumber
);