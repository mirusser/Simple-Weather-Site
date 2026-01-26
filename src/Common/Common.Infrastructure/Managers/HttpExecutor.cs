using Common.Infrastructure.Managers.Contracts;
using Polly.Registry;

namespace Common.Infrastructure.Managers;

public sealed class HttpExecutor(
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider)
    : IHttpExecutor
{
    public async Task<HttpResponseMessage> SendAsync(
        string clientName,
        string pipelineName,
        HttpRequestMessage request,
        CancellationToken ct)
    {
        var client = clientFactory.CreateClient(clientName);
        var pipeline = pipelineProvider.GetPipeline(pipelineName);

        return await pipeline.ExecuteAsync(
            async token => await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token),
            ct);
    }
}