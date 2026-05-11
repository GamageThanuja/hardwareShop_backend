using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record CreateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public CommonStatus Status { get; init; } = CommonStatus.Active;
}
