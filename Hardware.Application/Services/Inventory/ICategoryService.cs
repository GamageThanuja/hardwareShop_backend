using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Services.Inventory;

public interface ICategoryService
{
    Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<CategoryDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
