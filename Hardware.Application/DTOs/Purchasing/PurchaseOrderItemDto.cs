namespace Hardware.Application.DTOs.Purchasing;

public sealed record PurchaseOrderItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSKU { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public int ReceivedQuantity { get; init; }
    public decimal SubTotal { get; init; }
}
