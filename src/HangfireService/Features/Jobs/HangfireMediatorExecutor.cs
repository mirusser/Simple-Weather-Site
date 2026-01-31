using Common.Mediator;

namespace HangfireService.Features.Jobs;

public sealed class HangfireMediatorExecutor(IMediator mediator)
{
    public Task Execute(IRequest<bool> request)
        => mediator.SendAsync(request, CancellationToken.None);
}
