namespace Hardware.Application.DTOs.Users;

public sealed record UpdateUserDto(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive);
