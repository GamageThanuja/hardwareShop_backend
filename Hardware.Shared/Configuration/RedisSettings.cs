namespace Hardware.Shared.Configuration;

public sealed record RedisSettings
{
    public const string SectionName = "Redis";

    public string InstanceName { get; init; } = "Hardware:";
    public int DefaultExpirationMinutes { get; init; } = 30;
    public int ConnectTimeoutMs { get; init; } = 10_000;
    public int SyncTimeoutMs { get; init; } = 5_000;
}
