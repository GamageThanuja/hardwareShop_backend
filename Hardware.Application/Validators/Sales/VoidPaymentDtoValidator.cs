using FluentValidation;
using Hardware.Application.DTOs.Sales;

namespace Hardware.Application.Validators.Sales;

public sealed class VoidPaymentDtoValidator : AbstractValidator<VoidPaymentDto>
{
    public VoidPaymentDtoValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
