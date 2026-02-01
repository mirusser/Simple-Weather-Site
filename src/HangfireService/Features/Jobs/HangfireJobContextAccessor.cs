using Hangfire.Server;

namespace HangfireService.Features.Jobs;

public sealed class HangfireJobContextAccessor : IHangfireJobContextAccessor
{
    private static readonly AsyncLocal<PerformContext?> Current = new();

    public PerformContext? Context
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}
