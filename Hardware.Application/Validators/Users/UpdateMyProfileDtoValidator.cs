using FluentValidation;
using Hardware.Application.DTOs.Users;

namespace Hardware.Application.Validators.Users;

public sealed class UpdateMyProfileDtoValidator : AbstractValidator<UpdateMyProfileDto>
{
    public UpdateMyProfileDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => x.PhoneNumber is not null);
    }
}
