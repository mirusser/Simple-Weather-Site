namespace Common.Application.HealthChecks;

public sealed class ExternalEndpointHealthCheckOptions
{
    public required string ClientName { get; init; }
    public required string PipelineName { get; init; }

    // Use relative "/" if the named HttpClient has BaseAddress set
    public Uri Target { get; init; } = new Uri("/", UriKind.Relative);

    public HttpMethod Method { get; init; } = HttpMethod.Head;

    // If true, any HTTP response = healthy (even 401/404)
    public bool AnyHttpStatusIsHealthy { get; init; } = true;

    public string? HealthyMessage { get; init; }
}