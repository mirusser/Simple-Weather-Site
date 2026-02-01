using Common.Mediator;
using Hangfire;
namespace HangfireService.Features.Jobs;

public sealed class HangfireMediatorExecutor(
    IMediator mediator)
{
    public Task Execute(IRequest<bool> request)
        => mediator.SendAsync(request, CancellationToken.None);

    [JobDisplayName("{0}")]
    public async Task ExecuteNamed(string jobDisplayName, IRequest<bool> request)
        => await mediator.SendAsync(request, CancellationToken.None);
}
