namespace Common.Infrastructure.Managers.Contracts;

public interface IHttpRequestFactory
{
    public HttpRequestMessage Create<T>(
        string url,
        T body,
        string? httpMethod = null,
        Dictionary<string, string>? headers = null,
        string? contentType = null);

    HttpRequestMessage Create(
        string url,
        string? httpMethod = null,
        string? jsonBody = null,
        Dictionary<string, string>? headers = null,
        string? contentType = null);
}