using FluentValidation;
using Hardware.Application.DTOs.Sales;

namespace Hardware.Application.Validators.Sales;

public sealed class CreateSalesOrderDtoValidator : AbstractValidator<CreateSalesOrderDto>
{
    public CreateSalesOrderDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.DiscountPercent).InclusiveBetween(0, 100);
            item.RuleFor(i => i.TaxPercent).GreaterThanOrEqualTo(0);
        });
    }
}
