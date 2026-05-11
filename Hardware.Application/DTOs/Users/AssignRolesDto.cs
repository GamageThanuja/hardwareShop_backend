namespace Hardware.Application.DTOs.Users;

public sealed record AssignRolesDto(IReadOnlyList<string> Roles);
