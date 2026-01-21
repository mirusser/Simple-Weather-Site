using Microsoft.Extensions.DependencyInjection;

namespace Common.Mediator.Wrappers;

internal abstract class RequestHandlerWrapper
{
    public abstract Task<object?> HandleAsync(object request, IServiceProvider sp, CancellationToken ct);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> HandleAsync(object request, IServiceProvider sp, CancellationToken ct)
    {
        // resolve handler
        var handler = sp.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        // resolve pipeline behaviors (order matters: outer -> inner)
        var behaviors = sp.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

        RequestHandlerDelegate<TResponse> next = () => handler.Handle((TRequest)request, ct);

        // wrap in reverse so behaviors[0] is outermost
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentNext = next;
            next = () => behavior.HandleAsync((TRequest)request, currentNext, ct);
        }

        var result = await next().ConfigureAwait(false);
        return result;
    }
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();