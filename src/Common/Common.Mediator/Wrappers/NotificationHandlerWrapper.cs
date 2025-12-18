using Microsoft.Extensions.DependencyInjection;

namespace Common.Mediator.Wrappers;

internal abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(object notification, IServiceProvider sp, CancellationToken ct);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override async Task Handle(object notification, IServiceProvider sp, CancellationToken ct)
    {
        var handlers = sp.GetServices<INotificationHandler<TNotification>>().ToArray();

        // pluggable strategies (sequential/parallel)
        foreach (var h in handlers)
            await h.Handle((TNotification)notification, ct).ConfigureAwait(false);
    }
}