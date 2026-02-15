using System.Collections.Concurrent;
using Common.Infrastructure.Managers.Contracts;

namespace CitiesService.Tests.TestDoubles;

public sealed class FakeCacheManager : ICacheManager
{
    private readonly ConcurrentDictionary<string, object?> store = new();

    public string? LastTryGetKey { get; private set; }
    public string? LastSetKey { get; private set; }
    public int SetCallCount { get; private set; }

    public Func<string, (bool IsSuccess, object? Value)>? TryGetOverride { get; set; }

    public Task SetAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        LastSetKey = key;
        SetCallCount++;
        store[key] = value;
        return Task.CompletedTask;
    }

    public Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        LastTryGetKey = key;

        if (TryGetOverride is not null)
        {
            var (isSuccess, value) = TryGetOverride(key);
            return Task.FromResult((isSuccess, (T?)value));
        }

        if (!store.TryGetValue(key, out var obj))
        {
            return Task.FromResult((false, default(T)));
        }

        return Task.FromResult((true, (T?)obj));
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> task,
        CancellationToken cancellationToken = default)
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
