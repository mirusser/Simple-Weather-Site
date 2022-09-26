using FluentValidation;
using IconService.Application.Icon.Get;

namespace IconService.Validators;

public class GetValidator : AbstractValidator<GetQuery>
{
    public GetValidator()
    {
        RuleFor(g => g.Icon)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("{PropertyName} should not be empty.")
            .Length(1, int.MaxValue).WithMessage("{PropertyName} should be at least 1 character long")
            .Must(c => c.TrimStart().TrimEnd().Length > 0).WithMessage("{PropertyName} should have at least 1 character that is not a white character");
    }
}