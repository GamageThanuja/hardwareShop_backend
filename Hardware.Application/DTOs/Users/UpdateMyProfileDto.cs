namespace Hardware.Application.DTOs.Users;

public sealed record UpdateMyProfileDto(
    string FirstName,
    string LastName,
    string? PhoneNumber);
