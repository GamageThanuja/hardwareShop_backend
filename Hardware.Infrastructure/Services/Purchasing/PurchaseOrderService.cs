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
using Hardware.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Purchasing;

public sealed class PurchaseOrderService(IUnitOfWork uow, IMapper mapper, INotificationService notifications) : IPurchaseOrderService
{
    public async Task<PurchaseOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<PurchaseOrder>().Query()
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);
        return mapper.Map<PurchaseOrderDto>(entity);
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<PurchaseOrder> query = uow.Repository<PurchaseOrder>().Query()
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o => o.PONumber.Contains(request.Search) || o.Supplier.Name.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<PurchaseOrderDto>.Create(mapper.Map<IReadOnlyList<PurchaseOrderDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        if (!await uow.Repository<Supplier>().ExistsAsync(s => s.Id == dto.SupplierId, ct))
            throw new NotFoundException("Supplier", dto.SupplierId);

        var poNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new PurchaseOrder
        {
            PONumber = poNumber,
            SupplierId = dto.SupplierId,
            OrderDate = dto.OrderDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            Notes = dto.Notes,
            Status = PurchaseOrderStatus.Draft
        };

        decimal total = 0;
        foreach (var itemDto in dto.Items)
        {
            if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == itemDto.ProductId, ct))
                throw new NotFoundException("Product", itemDto.ProductId);

            var subTotal = itemDto.Quantity * itemDto.UnitCost;
            total += subTotal;

            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitCost = itemDto.UnitCost,
                SubTotal = subTotal
            });
        }

        order.TotalAmount = total;

        await uow.Repository<PurchaseOrder>().AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<PurchaseOrderDto> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        var entity = await uow.Repository<PurchaseOrder>().Query(tracking: true)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);

        if (entity.Status == PurchaseOrderStatus.Cancelled)
            throw new BusinessException("Cannot change status of a cancelled purchase order.");

        entity.Status = status;
        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<PurchaseOrderDto> ReceiveItemsAsync(Guid id, IReadOnlyList<ReceivePurchaseOrderItemDto> items, CancellationToken ct = default)
    {
        var order = await uow.Repository<PurchaseOrder>().Query(tracking: true)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);

        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
            throw new BusinessException("Cannot receive items for a completed or cancelled order.");

        foreach (var receiveDto in items)
        {
            var lineItem = order.Items.FirstOrDefault(i => i.Id == receiveDto.PurchaseOrderItemId)
                ?? throw new NotFoundException("PurchaseOrderItem", receiveDto.PurchaseOrderItemId);

            var remaining = lineItem.Quantity - lineItem.ReceivedQuantity;
            if (receiveDto.ReceivedQuantity > remaining)
                throw new BusinessException($"Cannot receive {receiveDto.ReceivedQuantity} units; only {remaining} remaining.");

            lineItem.ReceivedQuantity += receiveDto.ReceivedQuantity;
        }

        var allReceived = order.Items.All(i => i.ReceivedQuantity >= i.Quantity);
        var anyReceived = order.Items.Any(i => i.ReceivedQuantity > 0);

        order.Status = allReceived ? PurchaseOrderStatus.Received
            : anyReceived ? PurchaseOrderStatus.PartiallyReceived
            : order.Status;

        await uow.SaveChangesAsync(ct);

        if (allReceived)
        {
            var supplierName = await uow.Repository<Supplier>().Query(tracking: false)
                .Where(s => s.Id == order.SupplierId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
            _ = notifications.NotifyPurchaseOrderReceivedAsync(order.Id, order.PONumber, supplierName);
        }

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<PurchaseOrder>().Query(tracking: true)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("PurchaseOrder", id);

        if (entity.Status != PurchaseOrderStatus.Draft)
            throw new BusinessException("Only draft purchase orders can be deleted.");

        uow.Repository<PurchaseOrder>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
