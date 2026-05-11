using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Sales;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Hardware.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Sales;

public sealed class SalesReturnService(IUnitOfWork uow, IMapper mapper, INotificationService notifications) : ISalesReturnService
{
    public async Task<PagedResult<SalesReturnDto>> GetAllAsync(PagedRequestDto request, Guid? salesOrderId, CancellationToken ct = default)
    {
        IQueryable<SalesReturn> query = uow.Repository<SalesReturn>().Query(tracking: false)
            .Include(r => r.SalesOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product);

        if (salesOrderId.HasValue)
            query = query.Where(r => r.SalesOrderId == salesOrderId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReturnDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<SalesReturnDto>.Create(mapper.Map<IReadOnlyList<SalesReturnDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<SalesReturnDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<SalesReturn>().Query(tracking: false)
            .Include(r => r.SalesOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("SalesReturn", id);

        return mapper.Map<SalesReturnDto>(entity);
    }

    public async Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        var order = await uow.Repository<SalesOrder>().Query()
            .FirstOrDefaultAsync(o => o.Id == dto.SalesOrderId, ct)
            ?? throw new NotFoundException("SalesOrder", dto.SalesOrderId);

        if (order.Status is SalesOrderStatus.Draft or SalesOrderStatus.Cancelled)
            throw new BusinessException($"Cannot create a return for a {order.Status} order.");

        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.WarehouseId, ct))
            throw new NotFoundException("Warehouse", dto.WarehouseId);

        var returnNumber = $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var salesReturn = new SalesReturn
        {
            ReturnNumber  = returnNumber,
            SalesOrderId  = dto.SalesOrderId,
            WarehouseId   = dto.WarehouseId,
            ReturnDate    = dto.ReturnDate ?? DateTime.UtcNow,
            Reason        = dto.Reason,
            Notes         = dto.Notes,
            Status        = SalesReturnStatus.Completed
        };

        foreach (var itemDto in dto.Items)
        {
            if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == itemDto.ProductId, ct))
                throw new NotFoundException("Product", itemDto.ProductId);

            salesReturn.Items.Add(new SalesReturnItem
            {
                ProductId = itemDto.ProductId,
                Quantity  = itemDto.Quantity,
                Notes     = itemDto.Notes
            });

            await AddOrUpdateStockAsync(itemDto.ProductId, dto.WarehouseId, itemDto.Quantity, ct);

            await uow.Repository<InventoryTransaction>().AddAsync(new InventoryTransaction
            {
                ProductId       = itemDto.ProductId,
                WarehouseId     = dto.WarehouseId,
                TransactionType = InventoryTransactionType.ReturnIn,
                Quantity        = itemDto.Quantity,
                ReferenceType   = "SalesReturn",
                Notes           = itemDto.Notes
            }, ct);
        }

        await uow.Repository<SalesReturn>().AddAsync(salesReturn, ct);
        await uow.SaveChangesAsync(ct);

        _ = notifications.NotifySalesReturnAsync(salesReturn.Id, salesReturn.ReturnNumber, order.OrderNumber);

        return await GetByIdAsync(salesReturn.Id, ct);
    }

    public async Task<SalesReturnDto> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var salesReturn = await uow.Repository<SalesReturn>().Query()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("SalesReturn", id);

        if (salesReturn.Status == SalesReturnStatus.Cancelled)
            throw new BusinessException("Return is already cancelled.");

        foreach (var item in salesReturn.Items)
        {
            var stockItem = await uow.Repository<StockItem>().Query()
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == salesReturn.WarehouseId, ct);

            if (stockItem is not null)
            {
                stockItem.QuantityOnHand = Math.Max(0, stockItem.QuantityOnHand - item.Quantity);
            }

            await uow.Repository<InventoryTransaction>().AddAsync(new InventoryTransaction
            {
                ProductId       = item.ProductId,
                WarehouseId     = salesReturn.WarehouseId,
                TransactionType = InventoryTransactionType.StockOut,
                Quantity        = item.Quantity,
                ReferenceType   = "SalesReturnCancellation",
                ReferenceId     = salesReturn.Id,
                Notes           = $"Cancellation of return {salesReturn.ReturnNumber}"
            }, ct);
        }

        salesReturn.Status = SalesReturnStatus.Cancelled;
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    private async Task AddOrUpdateStockAsync(Guid productId, Guid warehouseId, int quantity, CancellationToken ct)
    {
        var stockItem = await uow.Repository<StockItem>().Query()
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, ct);

        if (stockItem is null)
        {
            await uow.Repository<StockItem>().AddAsync(
                new StockItem { ProductId = productId, WarehouseId = warehouseId, QuantityOnHand = quantity }, ct);
        }
        else
        {
            stockItem.QuantityOnHand += quantity;
        }
    }
}
