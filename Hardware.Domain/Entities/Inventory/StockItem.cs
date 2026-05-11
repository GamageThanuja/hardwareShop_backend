using Hardware.Domain.Common;

namespace Hardware.Domain.Entities.Inventory;

public class StockItem : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
