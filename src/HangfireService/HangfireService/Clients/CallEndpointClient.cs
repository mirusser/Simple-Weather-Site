using HangfireService.Clients.Contracts;

namespace HangfireService.Clients;

public class CallEndpointClient(HttpClient httpClient) : ICallEndpointClient
{
	public async Task GetMethodAsync(string url, CancellationToken cancellation = default) 
		=> await httpClient.GetAsync(url, cancellation);
}