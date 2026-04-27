using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Telemetry;
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
    {
        using var activity = GraphQlTelemetry.StartActivity(CitiesTelemetryConventions.Operations.GraphQl.GetCities);
        GraphQlTelemetry.SetResult(activity, CitiesTelemetryConventions.ResultValues.Deferred);

        return repo.FindAll(
            searchExpression: _ => true,
            // stable deterministic order is important for cursor pagination
            orderByExpression: q => q.OrderBy(c => c.Id));
    }

    public async Task<CityInfo?> GetCityByDbIdAsync(
        int id,
        [Service] ICityRepository repo,
        CancellationToken ct)
    {
        using var activity = GraphQlTelemetry.StartActivity(CitiesTelemetryConventions.Operations.GraphQl.GetCityByDbId);

        try
        {
            var city = await repo.FindAsync(c => c.Id == id, cancellationToken: ct);
            GraphQlTelemetry.SetResult(
                activity,
                city is null
                    ? CitiesTelemetryConventions.ResultValues.NotFound
                    : CitiesTelemetryConventions.ResultValues.Success);

            return city;
        }
        catch (Exception ex)
        {
            GraphQlTelemetry.SetException(activity, ex);
            throw;
        }
    }

    public async Task<CityInfo?> GetCityByCityIdAsync(
        decimal cityId,
        [Service] ICityRepository repo,
        CancellationToken ct)
    {
        using var activity = GraphQlTelemetry.StartActivity(CitiesTelemetryConventions.Operations.GraphQl.GetCityByCityId);

        try
        {
            var city = await repo.FindAsync(c => c.CityId == cityId, cancellationToken: ct);
            GraphQlTelemetry.SetResult(
                activity,
                city is null
                    ? CitiesTelemetryConventions.ResultValues.NotFound
                    : CitiesTelemetryConventions.ResultValues.Success);

            return city;
        }
        catch (Exception ex)
        {
            GraphQlTelemetry.SetException(activity, ex);
            throw;
        }
    }
}
