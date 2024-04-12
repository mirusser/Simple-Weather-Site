namespace HangfireService.Clients.Contracts;

public interface ICallEndpointClient
{
    Task<HttpResponseMessage> GetMethodAsync(string url, CancellationToken cancellation = default);
}
