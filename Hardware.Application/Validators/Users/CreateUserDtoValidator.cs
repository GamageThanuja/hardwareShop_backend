using FluentValidation;
using Hardware.Application.DTOs.Users;
using Hardware.Shared.Constants;

namespace Hardware.Application.Validators.Users;

public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Username may only contain letters, digits, dots, underscores, and hyphens.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => x.PhoneNumber is not null);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role is required.");
        RuleForEach(x => x.Roles).Must(r => RoleConstants.All.Contains(r))
            .WithMessage(r => $"'{r}' is not a valid role.");
    }
}
