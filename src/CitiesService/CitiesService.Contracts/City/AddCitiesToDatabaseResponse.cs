namespace CitiesService.Contracts.City;

public record AddCitiesToDatabaseResponse
(
    bool IsSuccess,
    bool IsAlreadyAdded
);