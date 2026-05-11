using System.ComponentModel.DataAnnotations;

namespace Hardware.Shared.Configuration;

public sealed record JwtSettings
{
    public const string SectionName = "Jwt";

    [Required] [MinLength(1)] public string Issuer { get; init; } = string.Empty;

    [Required] [MinLength(1)] public string Audience { get; init; } = string.Empty;

    [Required] [MinLength(32)] public string SecurityKey { get; init; } = string.Empty;

    [Range(1, 1440)] public int AccessTokenExpirationMinutes { get; init; } = 30;

    [Range(1, 365)] public int RefreshTokenExpirationDays { get; init; } = 14;

    [Range(1, 365)] public int RefreshTokenAbsoluteLifetimeDays { get; init; } = 90;

    [Range(1, 100)] public int MaxActiveSessionsPerUser { get; init; } = 10;

    [Range(0, 50)] public int MaxFailedLoginAttempts { get; init; } = 5;
}
