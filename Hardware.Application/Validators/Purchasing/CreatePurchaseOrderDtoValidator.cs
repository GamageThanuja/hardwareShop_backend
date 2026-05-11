using FluentValidation;
using Hardware.Application.DTOs.Purchasing;

namespace Hardware.Application.Validators.Purchasing;

public sealed class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Purchase order must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
