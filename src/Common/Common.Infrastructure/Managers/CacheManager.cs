using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Common.Infrastructure.Managers.Contracts;

namespace Common.Infrastructure.Managers;

public class CacheManager(IDistributedCache cache) : ICacheManager
{
	private static string versionKey = "v1";

	private static readonly JsonSerializerOptions serializerOptions = new()
	{
		PropertyNamingPolicy = null,
		WriteIndented = true,
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	//TODO: get options from settings
	private static readonly DistributedCacheEntryOptions options =
		new DistributedCacheEntryOptions()
			.SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
			.SetSlidingExpiration(TimeSpan.FromMinutes(2));

	public async Task SetAsync<T>(
		string key,
		T value,
		CancellationToken cancellationToken = default)
	{
		var serialized = JsonSerializer.Serialize(value, serializerOptions);
		var bytes = Encoding.UTF8.GetBytes(serialized);

		key = SetFullKey(key);
		await cache.SetAsync(key, bytes, options, cancellationToken);
		return;
	}

	public async Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(
		string key,
		CancellationToken cancellationToken = default)
	{
		T? value = default;
		key = SetFullKey(key);

		var val = await cache.GetAsync(key, cancellationToken);

		if (val is null)
		{
			return (false, value);
		}

		value = JsonSerializer.Deserialize<T>(val, serializerOptions);

		return (true, value);
	}

	public async Task<T?> GetOrSetAsync<T>(
		string key,
		Func<Task<T>> task,
		CancellationToken cancellationToken = default)
	{
		key = SetFullKey(key);

		(bool isSuccess, T? value) = await TryGetValueAsync<T>(key, cancellationToken);

		if (isSuccess)
		{
			return value;
		}

		value = await task();

		if (value is not null)
		{
			await SetAsync<T>(key, value, cancellationToken);
		}

		return value;
	}

	public void InvalidateCache()
	{
		versionKey = $"v{DateTime.UtcNow.Ticks}";
	}

	private string SetFullKey(string key)
		=> $"{versionKey}{key}";
}
