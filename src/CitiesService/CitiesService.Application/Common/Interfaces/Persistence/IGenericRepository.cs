using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CitiesService.Application.Common.Interfaces.Persistence;

public interface IGenericRepository<T> where T : class
{
    Task<bool> TryAcquireSeedLockAsync(CancellationToken ct);
    
    IQueryable<T> FindAll(
        Expression<Func<T, bool>> searchExpression,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        int skipNumberOfRows = default,
        int takeNumberOfRows = default,
        List<string>? includes = null);

    Task<T> FindAsync(Expression<Func<T, bool>> searchExpression, List<string>? includes = null, CancellationToken cancellationToken = default);

    Task<bool> CheckIfExistsAsync(Expression<Func<T, bool>> searchExpression, CancellationToken cancellationToken = default);

    Task<bool> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    bool Update(T entity);

    bool Delete(T entity);

    Task<bool> SaveAsync(CancellationToken cancellationToken = default);
}