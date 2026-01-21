using FluentValidation;

namespace EmailService.Application.Email.Commands;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(c => c.To)
            .Cascade(CascadeMode.Stop)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.To));

        RuleFor(c => c.From)
            .Cascade(CascadeMode.Stop)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.From));

        RuleFor(c => c.Subject)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.Body)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100_000);
        
        RuleFor(c => c.Attachments)
            .Must(a => a is not { Count: > 5 });
        
        RuleForEach(c => c.Attachments)
            .ChildRules(a =>
            {
                a.RuleFor(f => f.Length).LessThanOrEqualTo(10 * 1024 * 1024);
            });
    }
}