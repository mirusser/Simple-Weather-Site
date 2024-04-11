namespace HangfireService.Clients.Contracts;

public interface ICallEndpointClient
{
    Task GetMethodAsync(string url, CancellationToken cancellation = default);
}
