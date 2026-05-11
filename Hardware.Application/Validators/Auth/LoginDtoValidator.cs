using FluentValidation;
using Hardware.Application.DTOs.Auth;

namespace Hardware.Application.Validators.Auth;

public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
