using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record SupplierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public CommonStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
