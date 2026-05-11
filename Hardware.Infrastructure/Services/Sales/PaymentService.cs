using AutoMapper;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Sales;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;
using Hardware.Domain.Interfaces.Repositories;
using Hardware.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Services.Sales;

public sealed class PaymentService(
    IUnitOfWork uow,
    IMapper mapper,
    ICurrentUserService currentUser) : IPaymentService
{
    public async Task<PagedResult<PaymentDto>> GetAllAsync(Guid? salesOrderId, PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<Payment> query = uow.Repository<Payment>().Query(tracking: false)
            .Include(p => p.SalesOrder);

        if (salesOrderId.HasValue)
            query = query.Where(p => p.SalesOrderId == salesOrderId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PagedResult<PaymentDto>.Create(mapper.Map<IReadOnlyList<PaymentDto>>(items), request.Page, request.PageSize, total);
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await uow.Repository<Payment>().Query(tracking: false)
            .Include(p => p.SalesOrder)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Payment", id);

        return mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> RecordAsync(RecordPaymentDto dto, CancellationToken ct = default)
    {
        var order = await uow.Repository<SalesOrder>().Query()
            .FirstOrDefaultAsync(o => o.Id == dto.SalesOrderId, ct)
            ?? throw new NotFoundException("SalesOrder", dto.SalesOrderId);

        if (order.Status is SalesOrderStatus.Draft or SalesOrderStatus.Cancelled)
            throw new BusinessException($"Cannot record payment on a {order.Status} order.");

        var remaining = order.GrandTotal - order.AmountPaid;
        if (dto.Amount > remaining)
            throw new BusinessException($"Payment amount {dto.Amount:F2} exceeds the remaining balance {remaining:F2}.");

        var payment = new Payment
        {
            SalesOrderId    = order.Id,
            Amount          = dto.Amount,
            Method          = dto.Method,
            PaymentDate     = dto.PaymentDate ?? DateTime.UtcNow,
            ReferenceNumber = dto.ReferenceNumber,
            Notes           = dto.Notes,
            CreatedByUserId = currentUser.UserId
        };

        order.AmountPaid     += dto.Amount;
        order.PaymentStatus  = CalculateStatus(order.GrandTotal, order.AmountPaid);

        await uow.Repository<Payment>().AddAsync(payment, ct);
        await uow.SaveChangesAsync(ct);

        payment.SalesOrder = order;
        return mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> VoidAsync(Guid id, VoidPaymentDto dto, CancellationToken ct = default)
    {
        var payment = await uow.Repository<Payment>().Query()
            .Include(p => p.SalesOrder)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Payment", id);

        if (payment.IsVoided)
            throw new BusinessException("Payment is already voided.");

        payment.IsVoided        = true;
        payment.VoidedAt        = DateTime.UtcNow;
        payment.VoidedByUserId  = currentUser.UserId;
        payment.VoidReason      = dto.Reason;

        var order = payment.SalesOrder;
        order.AmountPaid    -= payment.Amount;
        if (order.AmountPaid < 0) order.AmountPaid = 0;
        order.PaymentStatus  = CalculateStatus(order.GrandTotal, order.AmountPaid);

        await uow.SaveChangesAsync(ct);

        return mapper.Map<PaymentDto>(payment);
    }

    private static PaymentStatus CalculateStatus(decimal grandTotal, decimal amountPaid) =>
        amountPaid <= 0           ? PaymentStatus.Unpaid
        : amountPaid >= grandTotal ? PaymentStatus.Paid
        :                            PaymentStatus.PartiallyPaid;
}
