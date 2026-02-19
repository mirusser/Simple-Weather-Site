
namespace Common.Testing.Http;

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> for tests that want full control over the client.
/// </summary>
public sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
