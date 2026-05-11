namespace Hardware.Shared.Configuration;

public sealed record HangfireSettings
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; init; } = "/hangfire";
    public string KeyPrefix { get; init; } = "Hardware:hangfire:";
    public int WorkerCountMultiplier { get; init; } = 5;
    public string[] Queues { get; init; } = ["critical", "default", "notifications", "reports"];
}
