using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Services.Inventory;

public interface ISupplierService
{
    Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<SupplierDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
