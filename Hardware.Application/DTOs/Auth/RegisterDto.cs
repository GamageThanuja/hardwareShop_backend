namespace Hardware.Application.DTOs.Auth;

public sealed record RegisterDto(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role);
