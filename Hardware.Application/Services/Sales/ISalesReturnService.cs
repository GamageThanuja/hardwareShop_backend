using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;

namespace Hardware.Application.Services.Sales;

public interface ISalesReturnService
{
    Task<PagedResult<SalesReturnDto>> GetAllAsync(PagedRequestDto request, Guid? salesOrderId, CancellationToken ct = default);
    Task<SalesReturnDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default);
    Task<SalesReturnDto> CancelAsync(Guid id, CancellationToken ct = default);
}
