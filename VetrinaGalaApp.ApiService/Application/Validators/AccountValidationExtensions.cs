using FluentValidation;

namespace VetrinaGalaApp.ApiService.Application.Validators;

public static class AccountValidationExtensions
{
    public static IRuleBuilderOptions<T, string> PasswordValidator<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Must(password => password.Any(char.IsUpper))
                .WithMessage("Password must contain at least one uppercase letter.")
            .Must(password => password.Any(char.IsLower))
                .WithMessage("Password must contain at least one lowercase letter.")
            .Must(password => password.Any(char.IsDigit))
                .WithMessage("Password must contain at least one number.")
            .Must(password => password.Any(ch => !char.IsLetterOrDigit(ch)))
                .WithMessage("Password must contain at least one special character.");
    }
    public static IRuleBuilderOptions<T, string> UserNameValidator<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(username => !username.Any(ch => !char.IsLetterOrDigit(ch)))
            .WithMessage("Username cannot contain special characters")
            .MinimumLength(3)
            .MaximumLength(12);
    }
}
