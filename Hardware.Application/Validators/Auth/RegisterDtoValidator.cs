using FluentValidation;
using Hardware.Application.DTOs.Auth;
using Hardware.Shared.Constants;

namespace Hardware.Application.Validators.Auth;

public sealed class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().MinimumLength(3).MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_.-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8).MaximumLength(200)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone must be in E.164 format.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => RoleConstants.All.Contains(r))
            .WithMessage(r => $"Invalid role '{r.Role}'. Must be one of: {string.Join(", ", RoleConstants.All)}");
    }
}
