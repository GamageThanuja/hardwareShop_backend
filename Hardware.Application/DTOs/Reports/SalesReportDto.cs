using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Reports;

public sealed record SalesReportDto
{
    public DateTime DateFrom { get; init; }
    public DateTime DateTo { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalTax { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal TotalAmountPaid { get; init; }
    public decimal TotalOutstanding { get; init; }
    public IReadOnlyList<SalesStatusSummaryDto> ByStatus { get; init; } = [];
    public IReadOnlyList<PaymentMethodSummaryDto> ByPaymentMethod { get; init; } = [];
    public IReadOnlyList<TopProductDto> TopProducts { get; init; } = [];
}

public sealed record SalesStatusSummaryDto(SalesOrderStatus Status, int Count, decimal TotalAmount);

public sealed record PaymentMethodSummaryDto(PaymentMethod Method, int Count, decimal TotalAmount);

public sealed record TopProductDto(Guid ProductId, string SKU, string ProductName, int QuantitySold, decimal Revenue);
