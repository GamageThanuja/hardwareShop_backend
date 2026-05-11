using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Purchasing;

public sealed record PurchaseReturnDto
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = string.Empty;
    public Guid PurchaseOrderId { get; init; }
    public string PONumber { get; init; } = string.Empty;
    public string SupplierName { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public DateTime ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public PurchaseReturnStatus Status { get; init; }
    public IReadOnlyList<PurchaseReturnItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public sealed record PurchaseReturnItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSKU { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public string? Notes { get; init; }
}
