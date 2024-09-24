using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Infrastructure.Extensions;

public static class DistributedCacheExtensions
{
	private static JsonSerializerOptions serializerOptions = new()
	{
		PropertyNamingPolicy = null,
		WriteIndented = true,
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	//TODO: get options from settings
	private static DistributedCacheEntryOptions options =
		new DistributedCacheEntryOptions()
			.SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
			.SetSlidingExpiration(TimeSpan.FromMinutes(2));

	public static async Task SetAsync<T>(
		this IDistributedCache cache,
		string key,
		T value,
		CancellationToken cancellationToken = default)
	{
		var serialized = JsonSerializer.Serialize(value, serializerOptions);
		var bytes = Encoding.UTF8.GetBytes(serialized);

		await cache.SetAsync(key, bytes, options, cancellationToken);
		return;
	}

	public static async Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(
		this IDistributedCache cache,
		string key,
		CancellationToken cancellationToken = default)
	{
		T? value = default;

		var val = await cache.GetAsync(key, cancellationToken);

		if (val is null)
		{
			return (false, value);
		}

		value = JsonSerializer.Deserialize<T>(val, serializerOptions);

		return (true, value);
	}

	public static async Task<T?> GetOrSetAsync<T>(
		this IDistributedCache cache,
		string key,
		Func<Task<T>> task,
		CancellationToken cancellationToken = default)
	{
		var (isSuccess, value) = await cache.TryGetValueAsync<T?>(key, cancellationToken);

		if (isSuccess)
		{
			return value;
		}

		value = await task();

		if (value is not null)
		{
			await cache.SetAsync<T>(key, value, cancellationToken);
		}

		return value;
	}
}