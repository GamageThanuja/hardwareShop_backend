using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record CreateInventoryTransactionDto
{
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public InventoryTransactionType TransactionType { get; init; }
    public int Quantity { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? Notes { get; init; }
}
