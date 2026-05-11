using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
