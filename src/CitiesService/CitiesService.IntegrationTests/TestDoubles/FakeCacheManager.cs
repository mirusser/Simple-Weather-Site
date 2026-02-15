using System.Collections.Concurrent;
using Common.Infrastructure.Managers.Contracts;

namespace CitiesService.IntegrationTests.TestDoubles;

public sealed class FakeCacheManager : ICacheManager
{
    private readonly ConcurrentDictionary<string, object?> store = new();

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        store[key] = value;
        return Task.CompletedTask;
    }

    public Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!store.TryGetValue(key, out var obj) 
            ? (false, default(T)) 
            : (true, (T?)obj));
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> task, CancellationToken cancellationToken = default)
    {
        var (ok, value) = await TryGetValueAsync<T>(key, cancellationToken);
        if (ok)
        {
            return value;
        }

        var created = await task();
        await SetAsync(key, created, cancellationToken);
        return created;
    }

    public void InvalidateCache() => store.Clear();
}
