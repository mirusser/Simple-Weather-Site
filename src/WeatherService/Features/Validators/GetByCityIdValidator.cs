using FluentValidation;
using WeatherService.Features.Queries;

namespace WeatherService.Features.Validators;

public class GetByCityIdValidator : AbstractValidator<GetByCityIdQuery>
{
    public GetByCityIdValidator()
    {
        RuleFor(g => g.CityId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("{PropertyName} should be greater than zero.");
    }
}