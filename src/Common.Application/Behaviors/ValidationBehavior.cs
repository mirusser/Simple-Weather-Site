using ErrorOr;
using FluentValidation;
using MediatR;

namespace Common.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
{
    private readonly IValidator<TRequest>? validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        this.validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request, //to investigate/manipulate/log something before moving next in the pipeline
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next) // this delegate eventually invoke handler
    {
        if (validator is null)
        {
            //before the handler
            return await next();
            //after the handler
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

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