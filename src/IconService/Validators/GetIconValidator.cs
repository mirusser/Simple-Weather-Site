using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IconService.Messages.Queries;

namespace IconService.Validators
{
    public class GetIconValidator : AbstractValidator<GetIconQuery>
    {
        public GetIconValidator()
        {
            RuleFor(g => g.Icon)
                .Cascade(CascadeMode.Continue)
                .NotEmpty().WithMessage("{PropertyName} should not be empty.")
                .Length(1, int.MaxValue).WithMessage("{PropertyName} should be at least 1 character long")
                .Must(c => c.TrimStart().TrimEnd().Length > 0).WithMessage("{PropertyName} should have at least 1 character that is not a white character");
        }
    }
}
