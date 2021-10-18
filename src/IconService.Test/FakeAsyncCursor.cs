using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IconService.Test
{
    public class FakeAsyncCursor<TEntity> : IAsyncCursor<TEntity>
    {
        private IEnumerable<TEntity> items;

        public FakeAsyncCursor(IEnumerable<TEntity> items)
        {
            this.items = items;
        }

        public IEnumerable<TEntity> Current => items;

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool MoveNext(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
