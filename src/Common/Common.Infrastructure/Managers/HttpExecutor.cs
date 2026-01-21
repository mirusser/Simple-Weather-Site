using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Common.Infrastructure.Managers;

public sealed class HttpExecutor(
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    IOptions<ResiliencePipeline> pipelineOptions)
    : IHttpExecutor
{
    public async Task<HttpResponseMessage> SendAsync(
        string clientName,
        HttpRequestMessage request,
        CancellationToken ct)
    {
        var client = clientFactory.CreateClient(clientName);
        var pipeline = pipelineProvider.GetPipeline(pipelineOptions.Value.Name);

        return await pipeline.ExecuteAsync(
            async token => await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token),
            ct);
    }
}