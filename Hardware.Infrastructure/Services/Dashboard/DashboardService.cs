using Hardware.Application.DTOs.Dashboard;
using Hardware.Application.Services.Dashboard;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Dashboard;

public sealed class DashboardService(IUnitOfWork uow) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return new DashboardDto
        {
            Inventory             = await BuildInventorySummaryAsync(ct),
            TodaySales            = await BuildSalesSummaryAsync(todayStart, now, ct),
            MonthSales            = await BuildSalesSummaryAsync(monthStart, now, ct),
            PendingPurchaseOrders = await uow.Repository<PurchaseOrder>().CountAsync(o => o.Status == PurchaseOrderStatus.Sent, ct),
            DraftPurchaseOrders   = await uow.Repository<PurchaseOrder>().CountAsync(o => o.Status == PurchaseOrderStatus.Draft, ct),
            LowStockAlerts        = await BuildLowStockAlertsAsync(ct),
            RecentSalesOrders     = await BuildRecentOrdersAsync(ct)
        };
    }

    private async Task<InventorySummaryDto> BuildInventorySummaryAsync(CancellationToken ct)
    {
        return new InventorySummaryDto
        {
            TotalProducts   = await uow.Repository<Product>().CountAsync(cancellationToken: ct),
            ActiveProducts  = await uow.Repository<Product>().CountAsync(p => p.Status == CommonStatus.Active, ct),
            TotalCategories = await uow.Repository<Category>().CountAsync(cancellationToken: ct),
            TotalSuppliers  = await uow.Repository<Supplier>().CountAsync(cancellationToken: ct),
            TotalWarehouses = await uow.Repository<Warehouse>().CountAsync(cancellationToken: ct),
            TotalCustomers  = await uow.Repository<Customer>().CountAsync(cancellationToken: ct)
        };
    }

    private async Task<SalesSummaryDto> BuildSalesSummaryAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var orders = await uow.Repository<SalesOrder>().Query()
            .Where(o => o.OrderDate >= from && o.OrderDate <= to && o.Status != SalesOrderStatus.Cancelled)
            .ToListAsync(ct);

        return new SalesSummaryDto
        {
            OrderCount  = orders.Count,
            TotalAmount = orders.Sum(o => o.GrandTotal)
        };
    }

    private async Task<IReadOnlyList<LowStockAlertDto>> BuildLowStockAlertsAsync(CancellationToken ct)
    {
        return await uow.Repository<StockItem>().Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.QuantityOnHand <= s.Product.ReorderLevel && s.Product.Status == CommonStatus.Active)
            .OrderBy(s => s.QuantityOnHand)
            .Take(20)
            .Select(s => new LowStockAlertDto
            {
                ProductId      = s.ProductId,
                SKU            = s.Product.SKU,
                ProductName    = s.Product.Name,
                QuantityOnHand = s.QuantityOnHand,
                ReorderLevel   = s.Product.ReorderLevel,
                WarehouseName  = s.Warehouse.Name
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<RecentOrderDto>> BuildRecentOrdersAsync(CancellationToken ct)
    {
        return await uow.Repository<SalesOrder>().Query()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                Id           = o.Id,
                OrderNumber  = o.OrderNumber,
                CustomerName = o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : null,
                GrandTotal   = o.GrandTotal,
                Status       = (int)o.Status,
                OrderDate    = o.OrderDate
            })
            .ToListAsync(ct);
    }
}
