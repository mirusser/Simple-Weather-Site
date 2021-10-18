using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IconService.Services;
using MongoDB.Driver;

namespace IconService.Mongo.Repository
{
    public class MongoRepository<TMongoDocument> : IMongoRepository<TMongoDocument> where TMongoDocument : class
    {
        private readonly IMongoCollection<TMongoDocument> _mongoCollection;

        public MongoRepository(IMongoCollectionFactory<TMongoDocument> mongoCollectionFactory)
        {
            _mongoCollection = mongoCollectionFactory.Create();
        }

        public async Task<TMongoDocument?> FindOneAsync(Expression<Func<TMongoDocument, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellation = default)
        {
            return await _mongoCollection.Find(predicate, findOptions).FirstOrDefaultAsync(cancellation).ConfigureAwait(false);
        }
    }
}
