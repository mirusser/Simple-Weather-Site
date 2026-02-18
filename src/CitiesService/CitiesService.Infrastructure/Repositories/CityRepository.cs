using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Entities.Dtos;
using CitiesService.Infrastructure.Contexts;

namespace CitiesService.Infrastructure.Repositories;

public class CityRepository(ApplicationDbContext context) : GenericRepository<CityInfo>(context), ICityRepository
{
    public async Task<CityInfo?> PatchAsync(
        CityPatchDto cityPatch,
        CancellationToken ct = default)
    {
        var city = await FindAsync(x => x.Id == cityPatch.Id, cancellationToken: ct);

        if (city is null)
            return city;

        if (cityPatch.Name is not null)
            city.Name = cityPatch.Name.Trim();

        if (cityPatch.CountryCode is not null)
            city.CountryCode = cityPatch.CountryCode.Trim();

        if (cityPatch.State is not null)
            city.State = string.IsNullOrWhiteSpace(cityPatch.State) ? null : cityPatch.State.Trim();

        if (cityPatch.Lat.HasValue)
            city.Lat = cityPatch.Lat.Value;

        if (cityPatch.Lon.HasValue)
            city.Lon = cityPatch.Lon.Value;

        if (cityPatch.RowVersion is not null && cityPatch.RowVersion != Array.Empty<byte>())
            SetRowVersion(city, cityPatch.RowVersion);

        await SaveAsync(ct);

        return city;
    }


    public void SetRowVersion(CityInfo cityInfo, byte[] rowVersion)
    {
        Context.Entry(cityInfo).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }
}