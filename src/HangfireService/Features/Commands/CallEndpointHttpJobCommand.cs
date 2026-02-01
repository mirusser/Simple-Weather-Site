using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator;
using HangfireService.Features.Jobs;

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
    IHttpRequestFactory requestFactory,
    IHangfireJobContextAccessor jobContextAccessor,
    ILogger<CallEndpointHttpJobHandler> logger)
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

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        SetJobParameter(jobContextAccessor, "response", responseBody);
        LogResponse(logger, "CallEndpointHttpJob", responseBody);

        response.EnsureSuccessStatusCode();
        
        return true;
    }

    private static void SetJobParameter(
        IHangfireJobContextAccessor accessor,
        string key,
        string? value)
    {
        accessor.Context?.SetJobParameter(key, Truncate(value));
    }

    private static void LogResponse(ILogger logger, string name, string? responseBody)
    {
        logger.LogInformation("{JobName} response: {Response}", name, Truncate(responseBody));
    }

    private static string? Truncate(string? value, int maxLength = 4000)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
