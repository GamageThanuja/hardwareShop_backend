namespace Hardware.Application.DTOs.Purchasing;

public sealed record CreatePurchaseOrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
}
