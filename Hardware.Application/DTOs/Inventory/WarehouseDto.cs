using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record WarehouseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public CommonStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
