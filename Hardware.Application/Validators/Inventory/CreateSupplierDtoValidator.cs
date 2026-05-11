using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.ContactName).MaximumLength(100);
    }
}
