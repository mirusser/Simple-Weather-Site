namespace Common.Infrastructure.Managers.Contracts;

public interface IHttpExecutor
{
    Task<HttpResponseMessage> SendAsync(
        string clientName,
        string pipelineName,
        HttpRequestMessage request,
        CancellationToken ct);
}