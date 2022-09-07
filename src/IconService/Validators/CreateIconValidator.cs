using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using IconService.Messages.Commands;
using IconService.Mongo.Documents;
using IconService.Services;
using MongoDB.Driver;

namespace IconService.Validators
{
    public class CreateIconValidator : AbstractValidator<CreateIconCommand>
    {
        private readonly IMongoCollection<IconDocument> _iconCollection;

        public CreateIconValidator(IMongoCollectionFactory<IconDocument> mongoCollectionFactory)
        {
            _iconCollection = mongoCollectionFactory.Get();

            RuleFor(c => c.Name)
               .Cascade(CascadeMode.Stop)
               .Must(IsNotNullOrEmptyAndAtLeastOneNonWhiteCharacterLong).WithMessage("{PropertyName} should have at least 1 character that is not a white character")
               .MustAsync(async (name, cancellation) => await IsNameUnique(name, cancellation)).WithMessage("{PropertyName} must be unique");

            RuleFor(c => c.Description)
                .Cascade(CascadeMode.Stop)
                .Must(IsNotNullOrEmptyAndAtLeastOneNonWhiteCharacterLong).WithMessage("{PropertyName} should have at least 1 character that is not a white character");

            RuleFor(c => c.Icon)
                .Cascade(CascadeMode.Stop)
                .Must(IsNotNullOrEmptyAndAtLeastOneNonWhiteCharacterLong).WithMessage("{PropertyName} should have at least 1 character that is not a white character")
                .MustAsync(async (icon, cancellation) => await IsIconUnique(icon, cancellation)).WithMessage("{PropertyName} must be unique");

            RuleFor(c => c.FileContent)
                .Cascade(CascadeMode.Continue)
                .NotNull().WithMessage("{PropertyName} can't be null")
                .NotEmpty().WithMessage("{PropertyName} can't be empty");
        }

        private bool IsNotNullOrEmptyAndAtLeastOneNonWhiteCharacterLong(string? value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 0 && value.TrimStart().TrimEnd().Length > 0;
        }

        private async Task<bool> IsNameUnique(string? name, CancellationToken cancellation = default)
        {
            return !(await _iconCollection.FindAsync(i => i.Name == name, cancellationToken: cancellation)).Any(cancellationToken: cancellation);
        }

        private async Task<bool> IsIconUnique(string? icon, CancellationToken cancellation = default)
        {
            return !(await _iconCollection.FindAsync(i => i.Icon == icon, cancellationToken: cancellation)).Any(cancellationToken: cancellation);
        }

        //Two ways of checking if instance object is null :

        //protected override bool PreValidate(ValidationContext<CreateIconCommand> context, ValidationResult result)
        //{
        //    if (context.InstanceToValidate is null)
        //    {
        //        result.Errors.Add(new ValidationFailure("", "Please ensure a model was supplied."));
        //    }

        //    return context.InstanceToValidate is not null;
        //}

        //public override ValidationResult Validate(ValidationContext<CreateIconCommand> context)
        //{
        //    return context.InstanceToValidate is null
        //    ? new ValidationResult(new[] { new ValidationFailure("CreateIcon", "CreateIcon cannot be null") })
        //    : Validate(context.InstanceToValidate);
        //}
    }
}