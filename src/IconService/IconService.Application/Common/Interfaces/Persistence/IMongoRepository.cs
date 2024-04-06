using MongoDB.Driver;
using System.Linq.Expressions;

namespace IconService.Application.Common.Interfaces.Persistence;

/// <summary>
/// Wrapper around IMongoCollection
/// </summary>
/// <typeparam name="TMongoDocument"></typeparam>
public interface IMongoRepository<TMongoDocument> where TMongoDocument : class
{
    #region Read

    Task<TMongoDocument?> FindOneAsync(
        Expression<Func<TMongoDocument, bool>> predicate,
        FindOptions? findOptions = null,
        CancellationToken cancellation = default);

    Task<IEnumerable<TMongoDocument>> GetAllAsync(
        FindOptions<TMongoDocument, TMongoDocument>? findOptions = null,
        CancellationToken cancellation = default);

    #endregion Read

    #region Write

    Task<TMongoDocument> CreateOneAsync(
        TMongoDocument mongoDocument,
        InsertOneOptions? insertOneOptions = null,
        CancellationToken cancellation = default);

    #endregion Write
}