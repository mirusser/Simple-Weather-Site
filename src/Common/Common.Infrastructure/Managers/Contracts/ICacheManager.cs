using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure.Managers.Contracts;
public interface ICacheManager
{
	public Task SetAsync<T>(
			string key,
			T value,
			CancellationToken cancellationToken = default);

	public Task<(bool IsSuccess, T? Value)> TryGetValueAsync<T>(
		string key,
		CancellationToken cancellationToken = default);

	public Task<T?> GetOrSetAsync<T>(
		string key,
		Func<Task<T>> task,
		CancellationToken cancellationToken = default);

	public void InvalidateCache();
}