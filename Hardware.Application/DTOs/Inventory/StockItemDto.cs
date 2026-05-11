namespace Hardware.Application.DTOs.Inventory;

public sealed record StockItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSKU { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int QuantityOnHand { get; init; }
    public int QuantityReserved { get; init; }
    public int QuantityAvailable { get; init; }
}
