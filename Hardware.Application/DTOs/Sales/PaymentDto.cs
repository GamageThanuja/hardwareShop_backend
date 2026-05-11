using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record PaymentDto
{
    public Guid Id { get; init; }
    public Guid SalesOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public PaymentMethod Method { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public bool IsVoided { get; init; }
    public DateTime? VoidedAt { get; init; }
    public Guid? VoidedByUserId { get; init; }
    public string? VoidReason { get; init; }
    public DateTime CreatedAt { get; init; }
}
