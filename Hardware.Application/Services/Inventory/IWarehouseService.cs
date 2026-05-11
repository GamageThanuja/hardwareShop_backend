using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Services.Inventory;

public interface IWarehouseService
{
    Task<WarehouseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<WarehouseDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default);
    Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
