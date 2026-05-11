using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;

namespace Hardware.Application.Services.Sales;

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetAllAsync(Guid? salesOrderId, PagedRequestDto request, CancellationToken ct = default);
    Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentDto> RecordAsync(RecordPaymentDto dto, CancellationToken ct = default);
    Task<PaymentDto> VoidAsync(Guid id, VoidPaymentDto dto, CancellationToken ct = default);
}
