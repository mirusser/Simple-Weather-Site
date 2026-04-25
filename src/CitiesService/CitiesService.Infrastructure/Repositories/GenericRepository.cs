using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Exceptions;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext Context = context;
    private readonly DbSet<T> db = context.Set<T>();

    public IQueryable<T> FindAll(
        Expression<Func<T, bool>>? searchExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null)
        => ApplyQuery(
            searchExpression,
            orderByExpression,
            skipNumberOfRows,
            takeNumberOfRows,
            includes);

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? searchExpression,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null,
        CancellationToken cancellationToken = default)
        => await ApplyQuery(
                searchExpression,
                orderByExpression,
                skipNumberOfRows,
                takeNumberOfRows,
                includes)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? searchExpression = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = db;

        if (searchExpression != null)
        {
            query = query.Where(searchExpression);
        }

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<T> ApplyQuery(
        Expression<Func<T, bool>>? searchExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        int skipNumberOfRows = 0,
        int takeNumberOfRows = 0,
        List<string>? includes = null)
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

    public async Task<T?> FindAsync(
        Expression<Func<T, bool>> searchExpression,
        List<string>? includes = null,
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
        Expression<Func<T, bool>> searchExpression,
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
        int numberOfRowsAffected;

        try
        {
            numberOfRowsAffected = await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new PersistenceConcurrencyException(
                "A concurrency conflict occurred while saving persistence changes.",
                ex);
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceUpdateException(
                "A persistence update error occurred while saving changes.",
                ex);
        }

        return numberOfRowsAffected > 0;
    }
}
