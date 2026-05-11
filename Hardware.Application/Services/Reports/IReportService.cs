using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Reports;

namespace Hardware.Application.Services.Reports;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<InventoryValuationDto> GetInventoryValuationAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<PurchaseReportDto> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
