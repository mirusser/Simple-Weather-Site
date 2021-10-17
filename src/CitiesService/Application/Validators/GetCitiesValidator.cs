using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Queries;
using FluentValidation;

namespace Application.Validators
{
    public class GetCitiesValidator : AbstractValidator<GetCitiesQuery>
    {
        public GetCitiesValidator()
        {
            RuleFor(g => g.CityName)
                .Cascade(CascadeMode.Continue)
                .NotEmpty().WithMessage("{PropertyName} should be not empty.")
                .Length(2, int.MaxValue).WithMessage("{PropertyName} should be at least 2 characters long")
                .Must(c => c.TrimStart().TrimEnd().Length >= 2).WithMessage("{PropertyName} should be have at least 2 characters that are not white characters");

            RuleFor(g => g.Limit)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(1).WithMessage("{PropertyName} should be bigger than 0.")
                .LessThanOrEqualTo(100).WithMessage("{PropertyName} can't be bigger than 100 due to bandwidth limitation");
        }
    }
}
