using FluentValidation;
using Hardware.Application.DTOs.Users;
using Hardware.Shared.Constants;

namespace Hardware.Application.Validators.Users;

public sealed class AssignRolesDtoValidator : AbstractValidator<AssignRolesDto>
{
    public AssignRolesDtoValidator()
    {
        RuleFor(x => x.Roles).NotNull().WithMessage("Roles list is required.");
        RuleForEach(x => x.Roles).Must(r => RoleConstants.All.Contains(r))
            .WithMessage(r => $"'{r}' is not a valid role.");
    }
}
