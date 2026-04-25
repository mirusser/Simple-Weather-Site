using System;
using System.Threading;
using System.Threading.Tasks;

namespace CitiesService.Application.Common.Interfaces.Persistence;

public interface ISeedLockProvider
{
    Task<ISeedLockLease?> TryAcquireAsync(
        string resource,
        CancellationToken cancellationToken = default);
}

public interface ISeedLockLease : IAsyncDisposable;
