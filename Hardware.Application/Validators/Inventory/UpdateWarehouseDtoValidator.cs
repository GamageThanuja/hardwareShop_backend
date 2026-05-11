using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class UpdateWarehouseDtoValidator : AbstractValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}
