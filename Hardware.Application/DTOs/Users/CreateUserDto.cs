namespace Hardware.Application.DTOs.Users;

public sealed record CreateUserDto(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string? PhoneNumber,
    string Password,
    IReadOnlyList<string> Roles);
