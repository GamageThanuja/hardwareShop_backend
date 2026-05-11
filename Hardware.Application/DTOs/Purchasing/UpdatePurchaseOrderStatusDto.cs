using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Purchasing;

public sealed record UpdatePurchaseOrderStatusDto
{
    public PurchaseOrderStatus Status { get; init; }
}
