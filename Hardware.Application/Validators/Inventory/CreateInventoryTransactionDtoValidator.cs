using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class CreateInventoryTransactionDtoValidator : AbstractValidator<CreateInventoryTransactionDto>
{
    public CreateInventoryTransactionDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ReferenceType).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
