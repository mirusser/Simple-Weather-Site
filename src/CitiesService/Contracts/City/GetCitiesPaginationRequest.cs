namespace Contracts.City;

public record GetCitiesPaginationRequest
(
    int NumberOfCities,
    int PageNumber
);