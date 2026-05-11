using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;

namespace Hardware.Application.Services.Sales;

public interface ICustomerService
{
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
