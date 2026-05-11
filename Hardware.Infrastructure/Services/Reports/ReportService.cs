using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Reports;
using Hardware.Application.Services.Reports;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Reports;

public sealed class ReportService(IUnitOfWork uow) : IReportService
{
    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var orders = await uow.Repository<SalesOrder>().Query(tracking: false)
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .ToListAsync(ct);

        var byStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new SalesStatusSummaryDto(g.Key, g.Count(), g.Sum(o => o.GrandTotal)))
            .OrderBy(s => s.Status)
            .ToList();

        var payments = await uow.Repository<Payment>().Query(tracking: false)
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to && !p.IsVoided)
            .ToListAsync(ct);

        var byMethod = payments
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodSummaryDto(g.Key, g.Count(), g.Sum(p => p.Amount)))
            .OrderByDescending(m => m.TotalAmount)
            .ToList();

        var topProducts = await uow.Repository<SalesOrderItem>().Query(tracking: false)
            .Where(i => i.SalesOrder.OrderDate >= from
                     && i.SalesOrder.OrderDate <= to
                     && i.SalesOrder.Status != SalesOrderStatus.Cancelled)
            .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.SKU })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.SKU,
                g.Key.Name,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.SubTotal)))
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToListAsync(ct);

        var nonCancelledOrders = orders.Where(o => o.Status != SalesOrderStatus.Cancelled).ToList();

        return new SalesReportDto
        {
            DateFrom         = from,
            DateTo           = to,
            TotalOrders      = nonCancelledOrders.Count,
            TotalRevenue     = nonCancelledOrders.Sum(o => o.GrandTotal),
            TotalTax         = nonCancelledOrders.Sum(o => o.TaxAmount),
            TotalDiscount    = nonCancelledOrders.Sum(o => o.DiscountAmount),
            TotalAmountPaid  = nonCancelledOrders.Sum(o => o.AmountPaid),
            TotalOutstanding = nonCancelledOrders.Sum(o => o.Balance),
            ByStatus         = byStatus,
            ByPaymentMethod  = byMethod,
            TopProducts      = topProducts
        };
    }

    public async Task<InventoryValuationDto> GetInventoryValuationAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        var stockItems = await uow.Repository<StockItem>().Query(tracking: false)
            .Include(s => s.Product).ThenInclude(p => p.Category)
            .Where(s => s.Product.Status == CommonStatus.Active)
            .ToListAsync(ct);

        var grouped = stockItems
            .GroupBy(s => s.ProductId)
            .Select(g =>
            {
                var p = g.First().Product;
                var totalUnits = g.Sum(s => s.QuantityOnHand);
                return new InventoryValuationItemDto(
                    p.Id,
                    p.SKU,
                    p.Name,
                    p.Category.Name,
                    totalUnits,
                    p.CostPrice,
                    totalUnits * p.CostPrice);
            })
            .OrderByDescending(i => i.TotalValue)
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            grouped = grouped
                .Where(i => i.ProductName.ToLower().Contains(search) || i.SKU.ToLower().Contains(search))
                .ToList();
        }

        var totalCount  = grouped.Count;
        var totalUnitsAll = grouped.Sum(i => i.TotalUnits);
        var totalValue  = grouped.Sum(i => i.TotalValue);

        var paged = grouped
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new InventoryValuationDto
        {
            TotalProducts = totalCount,
            TotalUnits    = totalUnitsAll,
            TotalValue    = totalValue,
            Page          = request.Page,
            PageSize      = request.PageSize,
            TotalCount    = totalCount,
            Items         = paged
        };
    }

    public async Task<PurchaseReportDto> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var orders = await uow.Repository<PurchaseOrder>().Query(tracking: false)
            .Include(o => o.Supplier)
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .ToListAsync(ct);

        var byStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new PurchaseStatusSummaryDto(g.Key, g.Count(), g.Sum(o => o.TotalAmount)))
            .OrderBy(s => s.Status)
            .ToList();

        var bySupplier = orders
            .Where(o => o.Status != PurchaseOrderStatus.Cancelled)
            .GroupBy(o => new { o.SupplierId, o.Supplier.Name })
            .Select(g => new SupplierSpendDto(g.Key.SupplierId, g.Key.Name, g.Count(), g.Sum(o => o.TotalAmount)))
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToList();

        var nonCancelled = orders.Where(o => o.Status != PurchaseOrderStatus.Cancelled).ToList();

        return new PurchaseReportDto
        {
            DateFrom    = from,
            DateTo      = to,
            TotalOrders = nonCancelled.Count,
            TotalAmount = nonCancelled.Sum(o => o.TotalAmount),
            ByStatus    = byStatus,
            BySupplier  = bySupplier
        };
    }
}
