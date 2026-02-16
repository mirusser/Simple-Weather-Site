using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.GraphQL;

public class CityQueries
{
    [UsePaging(IncludeTotalCount = true, DefaultPageSize = 20, MaxPageSize = 100)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<CityInfo> GetCities(ApplicationDbContext db)
        => db.CityInfos
            .AsNoTracking()
            // stable deterministic order is important for cursor pagination
            .OrderBy(c => c.Id);

    public Task<CityInfo?> GetCityByDbIdAsync(
        int id,
        ApplicationDbContext db,
        CancellationToken ct)
        => db.CityInfos.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<CityInfo?> GetCityByCityIdAsync(
        decimal cityId,
        ApplicationDbContext db,
        CancellationToken ct)
        => db.CityInfos.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CityId == cityId, ct);
}