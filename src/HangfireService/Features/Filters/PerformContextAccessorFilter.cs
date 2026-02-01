using Hangfire.Server;
using HangfireService.Features.Jobs;

namespace HangfireService.Features.Filters;

public sealed class PerformContextAccessorFilter(IHangfireJobContextAccessor accessor) : IServerFilter
{
    public void OnPerforming(PerformingContext filterContext)
    {
        accessor.Context = filterContext;
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        accessor.Context = null;
    }
}
