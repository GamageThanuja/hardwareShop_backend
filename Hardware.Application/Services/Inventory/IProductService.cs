using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Services.Inventory;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
