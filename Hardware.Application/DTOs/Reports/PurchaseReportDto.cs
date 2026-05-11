using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Reports;

public sealed record PurchaseReportDto
{
    public DateTime DateFrom { get; init; }
    public DateTime DateTo { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<PurchaseStatusSummaryDto> ByStatus { get; init; } = [];
    public IReadOnlyList<SupplierSpendDto> BySupplier { get; init; } = [];
}

public sealed record PurchaseStatusSummaryDto(PurchaseOrderStatus Status, int Count, decimal TotalAmount);

public sealed record SupplierSpendDto(Guid SupplierId, string SupplierName, int OrderCount, decimal TotalAmount);
