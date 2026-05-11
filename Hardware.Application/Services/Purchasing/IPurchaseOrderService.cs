using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Purchasing;
using Hardware.Domain.Enums;

namespace Hardware.Application.Services.Purchasing;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<PurchaseOrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default);
    Task<PurchaseOrderDto> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default);
    Task<PurchaseOrderDto> ReceiveItemsAsync(Guid id, IReadOnlyList<ReceivePurchaseOrderItemDto> items, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
