using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Domain.Enums;

namespace Hardware.Application.Services.Sales;

public interface ISalesOrderService
{
    Task<SalesOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SalesOrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<SalesOrderDto> CreateAsync(CreateSalesOrderDto dto, CancellationToken ct = default);
    Task<SalesOrderDto> UpdateStatusAsync(Guid id, SalesOrderStatus status, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
