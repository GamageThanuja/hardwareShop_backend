using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record InventoryTransactionDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public InventoryTransactionType TransactionType { get; init; }
    public int Quantity { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
