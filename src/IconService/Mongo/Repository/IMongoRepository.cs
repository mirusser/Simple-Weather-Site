using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IconService.Mongo.Repository
{
    public interface IMongoRepository<TMongoDocument> where TMongoDocument : class
    {
        Task<TMongoDocument?> FindOneAsync(Expression<Func<TMongoDocument, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellation = default);
    }
}