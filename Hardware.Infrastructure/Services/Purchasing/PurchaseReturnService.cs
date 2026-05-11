using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Purchasing;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Purchasing;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Purchasing;

public sealed class PurchaseReturnService(IUnitOfWork uow, IMapper mapper) : IPurchaseReturnService
{
    public async Task<PagedResult<PurchaseReturnDto>> GetAllAsync(PagedRequestDto request, Guid? purchaseOrderId, CancellationToken ct = default)
    {
        IQueryable<PurchaseReturn> query = uow.Repository<PurchaseReturn>().Query(tracking: false)
            .Include(r => r.PurchaseOrder).ThenInclude(po => po.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Product);

        if (purchaseOrderId.HasValue)
            query = query.Where(r => r.PurchaseOrderId == purchaseOrderId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReturnDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<PurchaseReturnDto>.Create(mapper.Map<IReadOnlyList<PurchaseReturnDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<PurchaseReturnDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<PurchaseReturn>().Query(tracking: false)
            .Include(r => r.PurchaseOrder).ThenInclude(po => po.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("PurchaseReturn", id);

        return mapper.Map<PurchaseReturnDto>(entity);
    }

    public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        var order = await uow.Repository<PurchaseOrder>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == dto.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", dto.PurchaseOrderId);

        if (order.Status is not (PurchaseOrderStatus.Received or PurchaseOrderStatus.PartiallyReceived))
            throw new BusinessException($"Cannot create a return for a {order.Status} purchase order.");

        if (!await uow.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.WarehouseId, ct))
            throw new NotFoundException("Warehouse", dto.WarehouseId);

        var returnNumber = $"PR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var purchaseReturn = new PurchaseReturn
        {
            ReturnNumber    = returnNumber,
            PurchaseOrderId = dto.PurchaseOrderId,
            WarehouseId     = dto.WarehouseId,
            ReturnDate      = dto.ReturnDate ?? DateTime.UtcNow,
            Reason          = dto.Reason,
            Notes           = dto.Notes,
            Status          = PurchaseReturnStatus.Completed
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await uow.Repository<Product>().Query()
                .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId, ct)
                ?? throw new NotFoundException("Product", itemDto.ProductId);

            var stockItem = await uow.Repository<StockItem>().Query()
                .FirstOrDefaultAsync(s => s.ProductId == itemDto.ProductId && s.WarehouseId == dto.WarehouseId, ct);

            if (stockItem is null || stockItem.QuantityOnHand < itemDto.Quantity)
                throw new BusinessException($"Insufficient stock for product '{product.Name}'. Available: {stockItem?.QuantityOnHand ?? 0}, Requested: {itemDto.Quantity}.");

            stockItem.QuantityOnHand -= itemDto.Quantity;

            purchaseReturn.Items.Add(new PurchaseReturnItem
            {
                ProductId = itemDto.ProductId,
                Quantity  = itemDto.Quantity,
                UnitCost  = product.CostPrice,
                Notes     = itemDto.Notes
            });

            await uow.Repository<InventoryTransaction>().AddAsync(new InventoryTransaction
            {
                ProductId       = itemDto.ProductId,
                WarehouseId     = dto.WarehouseId,
                TransactionType = InventoryTransactionType.ReturnOut,
                Quantity        = itemDto.Quantity,
                ReferenceType   = "PurchaseReturn",
                Notes           = itemDto.Notes
            }, ct);
        }

        await uow.Repository<PurchaseReturn>().AddAsync(purchaseReturn, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(purchaseReturn.Id, ct);
    }

    public async Task<PurchaseReturnDto> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var purchaseReturn = await uow.Repository<PurchaseReturn>().Query()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("PurchaseReturn", id);

        if (purchaseReturn.Status == PurchaseReturnStatus.Cancelled)
            throw new BusinessException("Return is already cancelled.");

        foreach (var item in purchaseReturn.Items)
        {
            var stockItem = await uow.Repository<StockItem>().Query()
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == purchaseReturn.WarehouseId, ct);

            if (stockItem is not null)
                stockItem.QuantityOnHand += item.Quantity;

            await uow.Repository<InventoryTransaction>().AddAsync(new InventoryTransaction
            {
                ProductId       = item.ProductId,
                WarehouseId     = purchaseReturn.WarehouseId,
                TransactionType = InventoryTransactionType.StockIn,
                Quantity        = item.Quantity,
                ReferenceType   = "PurchaseReturnCancellation",
                ReferenceId     = purchaseReturn.Id,
                Notes           = $"Cancellation of return {purchaseReturn.ReturnNumber}"
            }, ct);
        }

        purchaseReturn.Status = PurchaseReturnStatus.Cancelled;
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }
}
