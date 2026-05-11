namespace Hardware.Application.DTOs.Inventory;

public sealed record CreateWarehouseDto
{
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
}
