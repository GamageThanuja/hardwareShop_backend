namespace Hardware.Application.DTOs.Purchasing;

public sealed record CreatePurchaseOrderDto
{
    public Guid SupplierId { get; init; }
    public DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<CreatePurchaseOrderItemDto> Items { get; init; } = [];
}
