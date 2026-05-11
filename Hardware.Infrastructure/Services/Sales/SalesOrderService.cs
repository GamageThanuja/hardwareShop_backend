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
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Sales;

public sealed class SalesOrderService(IUnitOfWork uow, IMapper mapper) : ISalesOrderService
{
    public async Task<SalesOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<SalesOrder>().Query()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("SalesOrder", id);
        return mapper.Map<SalesOrderDto>(entity);
    }

    public async Task<PagedResult<SalesOrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<SalesOrder> query = uow.Repository<SalesOrder>().Query()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o => o.OrderNumber.Contains(request.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<SalesOrderDto>.Create(mapper.Map<IReadOnlyList<SalesOrderDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderDto dto, CancellationToken ct = default)
    {
        var orderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate,
            Notes = dto.Notes,
            Status = SalesOrderStatus.Draft
        };

        decimal subTotal = 0;
        decimal taxAmount = 0;
        decimal discountAmount = 0;

        foreach (var itemDto in dto.Items)
        {
            if (!await uow.Repository<Product>().ExistsAsync(p => p.Id == itemDto.ProductId, ct))
                throw new NotFoundException("Product", itemDto.ProductId);

            var discount = itemDto.UnitPrice * itemDto.Quantity * itemDto.DiscountPercent / 100;
            var lineNet = itemDto.UnitPrice * itemDto.Quantity - discount;
            var tax = lineNet * itemDto.TaxPercent / 100;
            var lineTotal = lineNet + tax;

            subTotal += itemDto.UnitPrice * itemDto.Quantity;
            discountAmount += discount;
            taxAmount += tax;

            order.Items.Add(new SalesOrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                DiscountPercent = itemDto.DiscountPercent,
                TaxPercent = itemDto.TaxPercent,
                SubTotal = lineTotal
            });
        }

        order.SubTotal = subTotal;
        order.DiscountAmount = discountAmount;
        order.TaxAmount = taxAmount;
        order.GrandTotal = subTotal - discountAmount + taxAmount;

        await uow.Repository<SalesOrder>().AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<SalesOrderDto> UpdateStatusAsync(Guid id, SalesOrderStatus status, CancellationToken ct = default)
    {
        var entity = await uow.Repository<SalesOrder>().Query(tracking: true)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("SalesOrder", id);

        if (entity.Status == SalesOrderStatus.Cancelled)
            throw new BusinessException("Cannot change status of a cancelled order.");

        entity.Status = status;
        await uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await uow.Repository<SalesOrder>().Query(tracking: true)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("SalesOrder", id);

        if (entity.Status != SalesOrderStatus.Draft)
            throw new BusinessException("Only draft orders can be deleted.");

        uow.Repository<SalesOrder>().Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
