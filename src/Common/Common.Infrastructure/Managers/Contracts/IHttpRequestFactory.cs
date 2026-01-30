namespace Common.Infrastructure.Managers.Contracts;

public interface IHttpRequestFactory
{
    HttpRequestMessage Create(
        string url,
        string? httpMethod = null,
        string? jsonBody = null,
        Dictionary<string, string>? headers = null,
        string? contentType = null);
}
