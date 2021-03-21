using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CitiesService.Logic.Repositories.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IQueryable<T>> FindAll(
            Expression<Func<T, bool>> searchExpression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression = null,
            List<string> includes = null);

        Task<T> Find(Expression<Func<T, bool>> searchExpression = null, List<string> includes = null);
        Task<bool> CheckIfExists(Expression<Func<T, bool>> searchExpression = null);

        Task<bool> Create(T entity);
        Task<bool> CreateRange(IEnumerable<T> entities);
        bool Update(T entity);
        bool Delete(T entity);

        Task<bool> Save();
    }
}
