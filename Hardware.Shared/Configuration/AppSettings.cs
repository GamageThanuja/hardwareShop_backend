namespace Hardware.Shared.Configuration;

public sealed record AppSettings
{
    public const string SectionName = "AppSettings";

    public string ApplicationName { get; init; } = "Hardware";
    public string Version { get; init; } = "1.0.0";
    public string Environment { get; init; } = "Development";
    public string? ApiBaseUrl { get; init; }
    public string? FrontendBaseUrl { get; init; }
    public bool EnableSwagger { get; init; } = true;
    public int SlowQueryThresholdMs { get; init; } = 1000;
}
