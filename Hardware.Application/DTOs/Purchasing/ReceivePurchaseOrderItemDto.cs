namespace Hardware.Application.DTOs.Purchasing;

public sealed record ReceivePurchaseOrderItemDto
{
    public Guid PurchaseOrderItemId { get; init; }
    public int ReceivedQuantity { get; init; }
}
