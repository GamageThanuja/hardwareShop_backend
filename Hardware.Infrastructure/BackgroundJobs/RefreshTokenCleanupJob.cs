using Hardware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hardware.Infrastructure.BackgroundJobs;

public sealed class RefreshTokenCleanupJob(
    ApplicationDbContext db,
    ILogger<RefreshTokenCleanupJob> logger)
{
    private static readonly TimeSpan RetentionWindow = TimeSpan.FromDays(30);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - RetentionWindow;

        var deleted = await db.RefreshTokens
            .Where(t => (t.RevokedUtc != null && t.RevokedUtc < cutoff)
                        || t.AbsoluteExpiresUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            logger.LogInformation("RefreshToken cleanup removed {Count} rows older than {Cutoff:O}", deleted, cutoff);
    }
}
