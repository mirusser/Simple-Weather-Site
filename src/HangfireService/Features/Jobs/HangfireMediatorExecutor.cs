using Common.Mediator;

namespace HangfireService.Features.Jobs;

using Hangfire;

public sealed class HangfireMediatorExecutor(IMediator mediator)
{
    public Task Execute(IRequest<bool> request)
        => mediator.SendAsync(request, CancellationToken.None);

    [JobDisplayName("{0}")]
    public Task ExecuteNamed(string jobDisplayName, IRequest<bool> request)
        => mediator.SendAsync(request, CancellationToken.None);
}
