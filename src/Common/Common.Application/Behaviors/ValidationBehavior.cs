using Common.Mediator;
using Common.Mediator.Wrappers;
using FluentValidation;

namespace Common.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (validator is null)
        {
            //before the handler
            return await next();
            //after the handler
        }

        var result = await validator.ValidateAsync(request, ct);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        return await next();
    }
}