using FluentValidation;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Validators.Inventory;

public sealed class TransferStockDtoValidator : AbstractValidator<TransferStockDto>
{
    public TransferStockDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FromWarehouseId).NotEmpty();
        RuleFor(x => x.ToWarehouseId).NotEmpty()
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouses must be different.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Transfer quantity must be greater than zero.");
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
