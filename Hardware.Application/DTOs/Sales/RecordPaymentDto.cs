using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record RecordPaymentDto(
    Guid SalesOrderId,
    decimal Amount,
    PaymentMethod Method,
    DateTime? PaymentDate,
    string? ReferenceNumber,
    string? Notes);
