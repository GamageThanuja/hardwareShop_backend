namespace Hardware.Application.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles);