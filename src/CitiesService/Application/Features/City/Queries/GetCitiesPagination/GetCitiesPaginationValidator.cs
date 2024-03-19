using FluentValidation;

namespace Application.Features.City.Queries.GetCitiesPagination;

public class GetCitiesPaginationValidator : AbstractValidator<GetCitiesPaginationQuery>
{
    public GetCitiesPaginationValidator()
    {
        RuleFor(g => g.NumberOfCities)
            .Cascade(CascadeMode.Continue)
            .GreaterThanOrEqualTo(1).WithMessage("{PropertyName} should be bigger than 0.")
            .LessThanOrEqualTo(100).WithMessage("{PropertyName} can't be bigger than 100 due to bandwidth limitation");

        RuleFor(g => g.PageNumber)
            .Cascade(CascadeMode.Continue)
            .GreaterThanOrEqualTo(1).WithMessage("{PropertyName} should be bigger than 0.");
    }
}