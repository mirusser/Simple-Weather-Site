using System.Collections.Concurrent;
using Common.Infrastructure.Managers.Contracts;

namespace Common.Testing.TestDoubles;

/// <summary>
/// Simple in-memory implementation of <see cref="ICacheManager"/> for tests.
///
/// Why: Most unit/integration tests want deterministic caching behavior without Redis.
/// This fake also provides lightweight observability (last keys + set call count).
/// </summary>
public sealed class FakeCacheManager : ICacheManager
{
    private readonly ConcurrentDictionary<string, object?> store = new();

    public string? LastTryGetKey { get; private set; }
    public string? LastSetKey { get; private set; }
    public int SetCallCount { get; private set; }

    /// <summary>
    /// Optional override to force cache hit/miss behavior.
    /// Return (true, value) to simulate a cache hit.
    /// </summary>
    public Func<string, (bool IsSuccess, object? Value)>? TryGetOverride { get; set; }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        LastSetKey = key;
        SetCallCount++;
        store[key] = value;
        return Task.CompletedTask;
    }

    public Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        LastTryGetKey = key;

        if (TryGetOverride is not null)
        {
            var (isSuccess, value) = TryGetOverride(key);
            return Task.FromResult((isSuccess, (T?)value));
        }

        return Task.FromResult(!store.TryGetValue(key, out var obj)
            ? (false, default(T))
            : (true, (T?)obj));
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> task, CancellationToken cancellationToken = default)
    {
        var (isSuccess, value) = await TryGetValueAsync<T>(key, cancellationToken);
        if (isSuccess && value is not null)
        {
            return value;
        }

        var created = await task();
        await SetAsync(key, created, cancellationToken);
        return created;
    }

    public void InvalidateCache() => store.Clear();
}
