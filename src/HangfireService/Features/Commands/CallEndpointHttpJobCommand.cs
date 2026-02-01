using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator;

namespace HangfireService.Features.Commands;

public sealed class CallEndpointHttpJobCommand : IRequest<bool>
{
    public string JobName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string HttpMethod { get; set; } = "POST";
    public string? BodyJson { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}

public sealed class CallEndpointHttpJobHandler(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory)
    : IRequestHandler<CallEndpointHttpJobCommand, bool>
{
    public async Task<bool> Handle(CallEndpointHttpJobCommand request, CancellationToken cancellationToken)
    {
        using var httpRequest = requestFactory.Create(
            request.Url,
            request.HttpMethod,
            request.BodyJson,
            request.Headers);

        var response = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName,
            PipelineNames.Default,
            httpRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        
        return true;
    }
}