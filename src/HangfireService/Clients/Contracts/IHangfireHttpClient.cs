namespace HangfireService.Clients.Contracts;

public interface IHangfireHttpClient
{
	Task<HttpResponseMessage> GetMethodAsync(
		string url,
		CancellationToken cancellation = default);

	Task<HttpResponseMessage> SendMethodAsync<T>(
		string url,
		T content,
		CancellationToken cancellation = default) where T : class, new();
}