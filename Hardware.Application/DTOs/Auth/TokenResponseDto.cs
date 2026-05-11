namespace Hardware.Application.DTOs.Auth;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles,
    string SessionId);
