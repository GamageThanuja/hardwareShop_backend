using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hardware.Infrastructure.BackgroundJobs;

public static class HangfireJobConfiguration
{
    public static void ConfigureRecurringJobs(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("HangfireJobs");
        var manager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        manager.AddOrUpdate<RefreshTokenCleanupJob>(
            "cleanup-refresh-tokens",
            "default",
            job => job.RunAsync(CancellationToken.None),
            "0 */6 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        logger.LogInformation("Hangfire recurring jobs configured");
    }
}
