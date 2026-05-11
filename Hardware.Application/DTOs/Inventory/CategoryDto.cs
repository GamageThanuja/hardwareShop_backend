using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public string? ParentCategoryName { get; init; }
    public CommonStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
