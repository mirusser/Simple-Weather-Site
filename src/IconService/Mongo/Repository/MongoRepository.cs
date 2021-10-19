using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IconService.Services;
using MongoDB.Driver;

namespace IconService.Mongo.Repository
{
    /// <summary>
    /// Wrapper around IMongoCollection
    /// </summary>
    /// <typeparam name="TMongoDocument"></typeparam>
    public class MongoRepository<TMongoDocument> : IMongoRepository<TMongoDocument> where TMongoDocument : class
    {
        private readonly IMongoCollection<TMongoDocument> _mongoCollection;

        public MongoRepository(IMongoCollectionFactory<TMongoDocument> mongoCollectionFactory)
        {
            _mongoCollection = mongoCollectionFactory.Get();
        }

        #region Read

        public async Task<TMongoDocument?> FindOneAsync(Expression<Func<TMongoDocument, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellation = default)
            => await _mongoCollection.Find(predicate, findOptions).FirstOrDefaultAsync(cancellation).ConfigureAwait(false);

        public async Task<IEnumerable<TMongoDocument>> GetAllAsync(FindOptions<TMongoDocument, TMongoDocument>? findOptions = null, CancellationToken cancellation = default)
            =>  (await _mongoCollection.FindAsync(_ => true, findOptions, cancellation)).ToEnumerable(cancellation);

        //TODO: other read methods here ...

        #endregion Read

        #region Write

        public async Task<TMongoDocument> CreateOneAsync(TMongoDocument mongoDocument, InsertOneOptions? insertOneOptions = null, CancellationToken cancellation = default)
        {
            await _mongoCollection.InsertOneAsync(mongoDocument, insertOneOptions, cancellation).ConfigureAwait(false);
            return mongoDocument;
        }

        //TODO: other write methods here ...

        #endregion Write
    }
}