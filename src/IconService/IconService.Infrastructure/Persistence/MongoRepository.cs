using System.Linq.Expressions;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IconService.Infrastructure.Persistence;

/// <summary>
/// Wrapper around IMongoCollection
/// </summary>
/// <typeparam name="TMongoDocument"></typeparam>
public class MongoRepository<TMongoDocument>(
    IMongoCollectionFactory<TMongoDocument> mongoCollectionFactory,
    IOptions<MongoSettings> options)
    : IMongoRepository<TMongoDocument>
    where TMongoDocument : class
{
    private readonly IMongoCollection<TMongoDocument> mongoCollection = mongoCollectionFactory.Get(options.Value.IconsCollectionName);

    #region Read

    public async Task<TMongoDocument?> FindOneAsync(
        Expression<Func<TMongoDocument, bool>> predicate,
        FindOptions? findOptions = null,
        CancellationToken cancellation = default)
        => await mongoCollection
            .Find(predicate, findOptions)
            .FirstOrDefaultAsync(cancellation)
            .ConfigureAwait(false);

    public async Task<IEnumerable<TMongoDocument>> GetAllAsync(
        FindOptions<TMongoDocument, TMongoDocument>? findOptions = null,
        CancellationToken cancellation = default)
        => (await mongoCollection
            .FindAsync(_ => true, findOptions, cancellation))
        .ToEnumerable(cancellation);

    //TODO: other read methods here ...

    #endregion Read

    #region Write

    public async Task<TMongoDocument> CreateOneAsync(
        TMongoDocument mongoDocument,
        InsertOneOptions? insertOneOptions = null,
        CancellationToken cancellation = default)
    {
        await mongoCollection
            .InsertOneAsync(mongoDocument, insertOneOptions, cancellation)
            .ConfigureAwait(false);

        return mongoDocument;
    }

    //TODO: other write methods here ...

    #endregion Write
}