using FluentValidation;
using WeatherService.Features.Queries;

namespace WeatherService.Features.Validators;

public class GetByCityNameFromXmlResponseValidator : AbstractValidator<GetByCityNameFromXmlResponseQuery>
{
    public GetByCityNameFromXmlResponseValidator()
    {
        RuleFor(g => g.City)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("{PropertyName} should not be null.")
            .NotEmpty().WithMessage("{PropertyName} should be not empty.")
            .Length(2, int.MaxValue).WithMessage("{PropertyName} should be at least 2 characters long")
            .Must(c => c.TrimStart().TrimEnd().Length >= 2)
            .WithMessage("{PropertyName} should be have at least 2 characters that are not white characters");
    }
}