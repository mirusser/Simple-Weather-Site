using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> db = context.Set<T>();
    
    public async Task<bool> TryAcquireSeedLockAsync(CancellationToken ct)
    {
        var conn = context.Database.GetDbConnection();
        await context.Database.OpenConnectionAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = 
            """
                DECLARE @result int;
                EXEC @result = sp_getapplock
                    @Resource = @resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = 0;
                SELECT @result;
            """;
        cmd.CommandType = System.Data.CommandType.Text;

        var p = cmd.CreateParameter();
        p.ParameterName = "@resource";
        p.Value = "CitiesSeed";
        cmd.Parameters.Add(p);

        var resultObj = await cmd.ExecuteScalarAsync(ct);
        var result = Convert.ToInt32(resultObj);

        // >= 0 means acquired (0 granted, 1 converted, etc.)
        return result >= 0;
    }

    public IQueryable<T> FindAll(
        Expression<Func<T, bool>> searchExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression = null,
        int skipNumberOfRows = default,
        int takeNumberOfRows = default,
        List<string> includes = null)
    {
        IQueryable<T> query = db;

        if (searchExpression != null)
        {
            query = query.Where(searchExpression);
        }

        if (includes?.Count > 0)
        {
            foreach (var table in includes)
            {
                query = query.Include(table);
            }
        }

        if (orderByExpression != null)
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

    public async Task<T> FindAsync(
        Expression<Func<T, bool>> searchExpression = null,
        List<string> includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = db;

        if (includes?.Count > 0)
        {
            foreach (var table in includes)
            {
                query = query.Include(table);
            }
        }

        return await query.FirstOrDefaultAsync(searchExpression, cancellationToken);
    }

    public async Task<bool> CheckIfExistsAsync(
        Expression<Func<T, bool>> searchExpression = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = db;

        return await query.AnyAsync(searchExpression, cancellationToken);
    }

    public async Task<bool> CreateAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        var entityEntry = await db.AddAsync(entity, cancellationToken);

        return entityEntry.State == EntityState.Added;
    }

    public async Task CreateRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        await db.AddRangeAsync(entities, cancellationToken);
    }

    public bool Delete(T entity)
    {
        var entityEntry = db.Remove(entity);

        return entityEntry.State == EntityState.Deleted;
    }

    public bool Update(T entity)
    {
        var entityEntry = db.Update(entity);

        return entityEntry.State == EntityState.Modified;
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        var numberOfRowsAffected = await context.SaveChangesAsync(cancellationToken);

        return numberOfRowsAffected > 0;
    }
}