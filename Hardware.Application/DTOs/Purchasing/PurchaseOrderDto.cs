using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Purchasing;

public sealed record PurchaseOrderDto
{
    public Guid Id { get; init; }
    public string PONumber { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public DateTime? ExpectedDeliveryDate { get; init; }
    public PurchaseOrderStatus Status { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<PurchaseOrderItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
