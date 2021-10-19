using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IconService.Mongo.Repository
{
    /// <summary>
    /// Wrapper around IMongoCollection
    /// </summary>
    /// <typeparam name="TMongoDocument"></typeparam>
    public interface IMongoRepository<TMongoDocument> where TMongoDocument : class
    {
        #region Read

        Task<TMongoDocument?> FindOneAsync(Expression<Func<TMongoDocument, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellation = default);

        Task<IEnumerable<TMongoDocument>> GetAllAsync(FindOptions<TMongoDocument, TMongoDocument>? findOptions = null, CancellationToken cancellation = default);

        #endregion Read

        #region Write

        Task<TMongoDocument> CreateOneAsync(TMongoDocument mongoDocument, InsertOneOptions? insertOneOptions = null, CancellationToken cancellation = default);

        #endregion Write
    }
}