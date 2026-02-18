using CitiesService.Domain.Entities;

namespace CitiesService.Application.Features.City.Models.Dto;

public sealed class UpdateCityResult
{
    public CityInfo City { get; init; } = null!;
}