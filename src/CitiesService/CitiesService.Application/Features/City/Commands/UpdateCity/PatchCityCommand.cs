using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Domain.Entities.Dtos;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Application.Features.City.Commands.UpdateCity;

public sealed class PatchCityCommand : IRequest<Result<UpdateCityResult>>
{
    public int Id { get; init; }
    public decimal? CityId { get; init; }
    public string? Name { get; init; } 
    public string? State { get; init; }
    public string? CountryCode { get; init; }
    public decimal? Lon { get; init; }
    public decimal? Lat { get; init; }
    public string? RowVersion { get; init; }
}

public sealed class PatchCityHandler(ICityRepository repo)
    : IRequestHandler<PatchCityCommand, Result<UpdateCityResult>>
{
    public async Task<Result<UpdateCityResult>> Handle(PatchCityCommand req, CancellationToken ct)
    {
        try
        {
            byte[]? rowVersion = null;
            if (!string.IsNullOrWhiteSpace(req.RowVersion))
            {
                rowVersion = Convert.FromBase64String(req.RowVersion);
            }
            
            var cityInfo = new CityPatchDto(
                Id: req.Id,
                CityId: req.CityId,
                Name: req.Name,
                State: req.State,
                CountryCode: req.CountryCode,
                Lon: req.Lon,
                Lat: req.Lat,
                RowVersion: rowVersion
            );

            var city = await repo.PatchAsync(cityInfo, ct);

            return city is null
                ? Result<UpdateCityResult>.Fail(Problems.NotFound($"City with Id={req.Id} not found"))
                : Result<UpdateCityResult>.Ok(new UpdateCityResult { City = city });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateCityResult>.Fail(
                Problems.Conflict(
                    "City was updated or deleted by another process. Refresh, check RowVersion, and retry."));
        }
    }
}