using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record CreateCustomerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public CustomerType CustomerType { get; init; } = CustomerType.Retail;
    public decimal CreditLimit { get; init; }
}
