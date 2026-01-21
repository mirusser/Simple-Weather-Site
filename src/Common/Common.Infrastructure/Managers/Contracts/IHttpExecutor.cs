namespace Common.Infrastructure.Managers.Contracts;

public interface IHttpExecutor
{
    Task<HttpResponseMessage> SendAsync(
        string clientName,
        HttpRequestMessage request,
        CancellationToken ct);
}