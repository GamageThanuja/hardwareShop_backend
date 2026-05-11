using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Purchasing;

namespace Hardware.Application.Services.Purchasing;

public interface IPurchaseReturnService
{
    Task<PagedResult<PurchaseReturnDto>> GetAllAsync(PagedRequestDto request, Guid? purchaseOrderId, CancellationToken ct = default);
    Task<PurchaseReturnDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default);
    Task<PurchaseReturnDto> CancelAsync(Guid id, CancellationToken ct = default);
}
