using System.Net.Mime;
using System.Text;
using System.Text.Json;
using HangfireService.Clients.Contracts;

namespace HangfireService.Clients;

public class HangfireHttpClient(
	HttpClient httpClient,
	JsonSerializerOptions jsonSerializerOptions) : IHangfireHttpClient
{
	public async Task<HttpResponseMessage> GetMethodAsync(
		string url,
		CancellationToken cancellation = default)
		=> await httpClient.GetAsync(url, cancellation);

	public async Task<HttpResponseMessage> SendMethodAsync<T>(
		string url,
		T content,
		CancellationToken cancellation = default) where T : class, new()
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
		{
			throw new ArgumentException($"Can't create Uri from provided url: {url}", nameof(url));
		}

		HttpRequestMessage request = new()
		{
			Method = HttpMethod.Post,
			RequestUri = uri,
			Content = new StringContent(
				JsonSerializer.Serialize(content, jsonSerializerOptions),
				Encoding.UTF8,
				MediaTypeNames.Application.Json)
		};

		return await httpClient.SendAsync(request, cancellation);
	}
}