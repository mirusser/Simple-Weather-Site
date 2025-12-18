using System.Collections.Concurrent;
using Common.Mediator.Wrappers;

namespace Common.Mediator;

public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly ConcurrentDictionary<Type, RequestHandlerWrapper> requestWrappers = new();
    private readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> notificationWrappers = new();

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();

        // Create wrapper once per concrete request type
        var wrapper = requestWrappers.GetOrAdd(requestType, static t =>
        {
            // Find IRequest<TResponse> implemented by the concrete type
            var iRequestType = t.GetInterfaces()
                           .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                       ?? throw new InvalidOperationException($"{t} does not implement IRequest<T>.");

            var responseType = iRequestType.GetGenericArguments()[0];
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(t, responseType);

            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        });

        var resultObj = await wrapper.HandleAsync(request, serviceProvider, ct).ConfigureAwait(false);
        return (TResponse)resultObj!;
    }

    public Task PublishAsync(INotification notification, CancellationToken ct = default)
    {
        var notifType = notification.GetType();

        var wrapper = notificationWrappers.GetOrAdd(notifType, static t =>
        {
            var wrapperType = typeof(NotificationHandlerWrapper<>).MakeGenericType(t);
            return (NotificationHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        });

        return wrapper.Handle(notification, serviceProvider, ct);
    }
}