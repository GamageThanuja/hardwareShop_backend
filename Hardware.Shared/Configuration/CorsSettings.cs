namespace Hardware.Shared.Configuration;

public sealed record CorsSettings
{
    public const string SectionName = "Cors";

    public string PolicyName { get; init; } = "Default";
    public string[] AllowedOrigins { get; init; } = [];
    public bool AllowCredentials { get; init; } = true;
}
