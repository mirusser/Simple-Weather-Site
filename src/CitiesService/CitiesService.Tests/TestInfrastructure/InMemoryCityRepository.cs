using System.Linq.Expressions;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Entities.Dtos;

namespace CitiesService.Tests.TestInfrastructure;

public sealed class InMemoryCityRepository(IEnumerable<CityInfo>? seed = null) : ICityRepository
{
    private readonly List<CityInfo> cities = seed?.ToList() ?? [];

    public IQueryable<CityInfo> FindAll(
        Expression<Func<CityInfo, bool>>? searchExpression,
        Func<IQueryable<CityInfo>, IOrderedQueryable<CityInfo>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null)
        => ApplyQuery(
            searchExpression,
            orderByExpression,
            skipNumberOfRows,
            takeNumberOfRows);

    public Task<List<CityInfo>> ListAsync(
        Expression<Func<CityInfo, bool>>? searchExpression,
        Func<IQueryable<CityInfo>, IOrderedQueryable<CityInfo>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(FindAll(
                searchExpression,
                orderByExpression,
                skipNumberOfRows,
                takeNumberOfRows,
                includes)
            .ToList());

    public Task<int> CountAsync(
        Expression<Func<CityInfo, bool>>? searchExpression = null,
        CancellationToken cancellationToken = default)
    {
        var query = cities.AsQueryable();
        if (searchExpression is not null)
        {
            query = query.Where(searchExpression);
        }

        return Task.FromResult(query.Count());
    }

    public Task<CityInfo?> FindAsync(
        Expression<Func<CityInfo, bool>> searchExpression,
        List<string>? includes = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(cities.AsQueryable().FirstOrDefault(searchExpression));

    public Task<bool> CheckIfExistsAsync(
        Expression<Func<CityInfo, bool>> searchExpression,
        CancellationToken cancellationToken = default)
        => Task.FromResult(cities.AsQueryable().Any(searchExpression));

    public Task<bool> CreateAsync(
        CityInfo entity,
        CancellationToken cancellationToken = default)
    {
        cities.Add(entity);
        return Task.FromResult(true);
    }

    public Task CreateRangeAsync(
        IEnumerable<CityInfo> entities,
        CancellationToken cancellationToken = default)
    {
        cities.AddRange(entities);
        return Task.CompletedTask;
    }

    public bool Update(CityInfo entity) => true;

    public bool Delete(CityInfo entity) => cities.Remove(entity);

    public Task<bool> SaveAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public async Task<CityInfo?> PatchAsync(CityPatchDto cityPatch, CancellationToken ct = default)
    {
        var city = await FindAsync(c => c.Id == cityPatch.Id, cancellationToken: ct);
        if (city is null)
        {
            return null;
        }

        if (cityPatch.Name is not null)
            city.Name = cityPatch.Name.Trim();
        if (cityPatch.State is not null)
            city.State = string.IsNullOrWhiteSpace(cityPatch.State) ? null : cityPatch.State.Trim();
        if (cityPatch.CountryCode is not null)
            city.CountryCode = cityPatch.CountryCode.Trim();
        if (cityPatch.Lon.HasValue)
            city.Lon = cityPatch.Lon.Value;
        if (cityPatch.Lat.HasValue)
            city.Lat = cityPatch.Lat.Value;
        if (cityPatch.RowVersion is not null)
            SetRowVersion(city, cityPatch.RowVersion);

        return city;
    }

    public void SetRowVersion(CityInfo cityInfo, byte[] rowVersion)
        => cityInfo.RowVersion = rowVersion;

    private IQueryable<CityInfo> ApplyQuery(
        Expression<Func<CityInfo, bool>>? searchExpression,
        Func<IQueryable<CityInfo>, IOrderedQueryable<CityInfo>>? orderByExpression,
        int skipNumberOfRows,
        int takeNumberOfRows)
    {
        IQueryable<CityInfo> query = cities.AsQueryable();

        if (searchExpression is not null)
        {
            query = query.Where(searchExpression);
        }

        if (orderByExpression is not null)
        {
            query = orderByExpression(query);
        }

        if (skipNumberOfRows > 0)
        {
            query = query.Skip(skipNumberOfRows);
        }

        if (takeNumberOfRows > 0)
        {
            query = query.Take(takeNumberOfRows);
        }

        return query;
    }
}
