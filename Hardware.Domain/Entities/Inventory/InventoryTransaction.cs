using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Inventory;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
