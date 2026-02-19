using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Entities.Dtos;

namespace CitiesService.Application.Common.Interfaces.Persistence;

public interface IGenericRepository<T> where T : class
{
    Task<bool> TryAcquireSeedLockAsync(CancellationToken ct);

    IQueryable<T> FindAll(
        Expression<Func<T, bool>>? searchExpression,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null);

    Task<T?> FindAsync(
        Expression<Func<T, bool>> searchExpression,
        List<string>? includes = null,
        CancellationToken cancellationToken = default);

    Task<bool> CheckIfExistsAsync(Expression<Func<T, bool>> searchExpression,
        CancellationToken cancellationToken = default);

    Task<bool> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    bool Update(T entity);

    bool Delete(T entity);

    Task<bool> SaveAsync(CancellationToken cancellationToken = default);
}

public interface ICityRepository : IGenericRepository<CityInfo>
{
    Task<CityInfo?> PatchAsync(CityPatchDto cityPatch, CancellationToken ct = default);

    void SetRowVersion(CityInfo cityInfo, byte[] rowVersion);
}