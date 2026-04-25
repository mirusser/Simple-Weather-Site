using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;

namespace CitiesService.GraphQL;

public class CityQueries
{
    public string Ping() => "pong";
    
    [UsePaging(IncludeTotalCount = true, DefaultPageSize = 20, MaxPageSize = 100)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<CityInfo> GetCities([Service] ICityRepository repo)
        => repo.FindAll(
            searchExpression: _ => true,
            // stable deterministic order is important for cursor pagination
            orderByExpression: q => q.OrderBy(c => c.Id));

    public Task<CityInfo?> GetCityByDbIdAsync(
        int id,
        [Service] ICityRepository repo,
        CancellationToken ct)
        => repo.FindAsync(c => c.Id == id, cancellationToken: ct);

    public Task<CityInfo?> GetCityByCityIdAsync(
        decimal cityId,
        [Service] ICityRepository repo,
        CancellationToken ct)
        => repo.FindAsync(c => c.CityId == cityId, cancellationToken: ct);
}
