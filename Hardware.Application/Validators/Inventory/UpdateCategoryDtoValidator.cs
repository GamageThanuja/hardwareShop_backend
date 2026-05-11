using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
