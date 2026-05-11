using FluentValidation;
using Hardware.Application.DTOs.Sales;
using Hardware.Domain.Enums;

namespace Hardware.Application.Validators.Sales;

public sealed class RecordPaymentDtoValidator : AbstractValidator<RecordPaymentDto>
{
    public RecordPaymentDtoValidator()
    {
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.Method).IsInEnum().WithMessage("Invalid payment method.");
        RuleFor(x => x.ReferenceNumber).MaximumLength(100).When(x => x.ReferenceNumber is not null);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
