
namespace Common.Testing.Http;

/// <summary>
/// Test HTTP handler that delegates to a function.
///
/// Why: lets tests fake HTTP responses without spinning up a real server.
/// </summary>
public sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => handler(request, cancellationToken);
}
