using FluentValidation;

namespace EmailService.Application.Email.Commands;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(c => c.To)
        .Cascade(CascadeMode.Continue)
        .NotNull()
        .NotEmpty()
        .EmailAddress();

        RuleFor(c => c.Subject)
        .Cascade(CascadeMode.Continue)
        .NotNull()
        .NotEmpty();

        RuleFor(c => c.Body)
        .Cascade(CascadeMode.Continue)
        .NotNull()
        .NotEmpty();

        When(c => !string.IsNullOrEmpty(c.From), () =>
        {
            RuleFor(c => c.From)
            .EmailAddress();
        });
    }
}