using Hangfire.Server;

namespace HangfireService.Features.Jobs;

public interface IHangfireJobContextAccessor
{
    PerformContext? Context { get; set; }
}
