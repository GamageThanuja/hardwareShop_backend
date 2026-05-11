namespace Hardware.Application.DTOs.Auth;

public sealed record SessionSummaryDto(
    string SessionId,
    string? DeviceName,
    string? ClientType,
    string? IpAddress,
    DateTime CreatedUtc,
    DateTime? LastUsedUtc,
    bool IsCurrent);
