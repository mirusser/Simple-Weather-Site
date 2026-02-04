using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Common.Infrastructure.Managers.Contracts;

namespace Common.Infrastructure.Managers;

public sealed class HttpRequestFactory(JsonSerializerOptions jsonSerializerOptions) : IHttpRequestFactory
{
    public HttpRequestMessage Create<T>(
        string url,
        T body,
        string? httpMethod = null,
        Dictionary<string, string>? headers = null,
        string? contentType = null)
    {
        var jsonBody = JsonSerializer.Serialize(
            body,
            jsonSerializerOptions);
        
        return Create(url, httpMethod, jsonBody, headers, contentType);
    }
    
    public HttpRequestMessage Create(
        string url,
        string? httpMethod = null,
        string? jsonBody = null,
        Dictionary<string, string>? headers = null,
        string? contentType = null)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Can't create Uri from provided url: {url}", nameof(url));
        }

        var method = new HttpMethod(httpMethod ?? HttpMethod.Get.Method);
        var request = new HttpRequestMessage(method, uri);

        if (!string.IsNullOrWhiteSpace(jsonBody))
        {
            request.Content = new StringContent(
                jsonBody,
                Encoding.UTF8,
                contentType ?? MediaTypeNames.Application.Json);
        }

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        return request;
    }
}
