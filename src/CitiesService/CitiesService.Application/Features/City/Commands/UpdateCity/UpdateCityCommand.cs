using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Models.Dto;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Application.Features.City.Commands.UpdateCity;

public sealed class UpdateCityCommand : IRequest<Result<UpdateCityResult>>
{
    public int Id { get; init; }
    public decimal CityId { get; init; }
    public string Name { get; init; } = null!;
    public string? State { get; init; }
    public string CountryCode { get; init; } = null!;
    public decimal Lon { get; init; }
    public decimal Lat { get; init; }
    public string? RowVersion { get; init; }
}

public sealed class UpdateCityHandler(ICityRepository repo)
    : IRequestHandler<UpdateCityCommand, Result<UpdateCityResult>>
{
    public async Task<Result<UpdateCityResult>> Handle(UpdateCityCommand request, CancellationToken ct)
    {
        var city = await repo
            .FindAsync(c => c.Id == request.Id, cancellationToken: ct);
        
        if (city is null)
        {
            return Result<UpdateCityResult>.Fail(Problems.NotFound($"City with Id={request.Id} not found"));
        }

        city.Name = request.Name.Trim();
        city.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim();
        city.CountryCode = request.CountryCode.Trim();
        city.Lon = request.Lon;
        city.Lat = request.Lat;

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
        {
            var rowVersion = Convert.FromBase64String(request.RowVersion);

            repo.SetRowVersion(city, rowVersion);
        }

        try
        {
            var saved = await repo.SaveAsync(ct);
            return saved
                ? Result<UpdateCityResult>.Ok(new UpdateCityResult { City = city })
                : Result<UpdateCityResult>.Fail(Problems.Conflict("Update affected 0 rows"));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateCityResult>.Fail(
                Problems.Conflict(
                    "City was updated or deleted by another process. Refresh, check RowVersion, and retry."));
        }
    }
}

