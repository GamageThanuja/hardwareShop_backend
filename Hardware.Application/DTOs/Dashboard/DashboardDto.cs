namespace Hardware.Application.DTOs.Dashboard;

public sealed record DashboardDto
{
    public InventorySummaryDto Inventory { get; init; } = new();
    public SalesSummaryDto TodaySales { get; init; } = new();
    public SalesSummaryDto MonthSales { get; init; } = new();
    public int PendingPurchaseOrders { get; init; }
    public int DraftPurchaseOrders { get; init; }
    public IReadOnlyList<LowStockAlertDto> LowStockAlerts { get; init; } = [];
    public IReadOnlyList<RecentOrderDto> RecentSalesOrders { get; init; } = [];
}

public sealed record InventorySummaryDto
{
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int TotalCategories { get; init; }
    public int TotalSuppliers { get; init; }
    public int TotalWarehouses { get; init; }
    public int TotalCustomers { get; init; }
}

public sealed record SalesSummaryDto
{
    public int OrderCount { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed record LowStockAlertDto
{
    public Guid ProductId { get; init; }
    public string SKU { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int QuantityOnHand { get; init; }
    public int ReorderLevel { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
}

public sealed record RecentOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string? CustomerName { get; init; }
    public decimal GrandTotal { get; init; }
    public int Status { get; init; }
    public DateTime OrderDate { get; init; }
}
