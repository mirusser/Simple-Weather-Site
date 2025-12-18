using Common.Mediator;
using Common.Mediator.Wrappers;
using ErrorOr;
using FluentValidation;

namespace Common.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
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

        var validationResult = await validator.ValidateAsync(request, ct);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(validationFailure => Error.Validation(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage));

        return (dynamic)errors;
    }
}