using Common.Mediator.Wrappers;

namespace Common.Mediator;

public interface INotification { }

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken ct);
}
