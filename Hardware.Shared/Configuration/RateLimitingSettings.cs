namespace Hardware.Shared.Configuration;

public sealed record RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int DefaultPerMinute { get; init; } = 100;
    public int AuthPerMinute { get; init; } = 5;
    public int AdminPerMinute { get; init; } = 200;
}
