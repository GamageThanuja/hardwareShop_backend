using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Inventory;

public class Warehouse : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    public ICollection<StockItem> StockItems { get; set; } = [];
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];
}
