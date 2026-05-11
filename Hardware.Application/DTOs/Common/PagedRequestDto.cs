using Hardware.Shared.Constants;

namespace Hardware.Application.DTOs.Common;

public sealed record PagedRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = AppConstants.DefaultPageSize;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public string? Search { get; init; }
}
