using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Hardware.Infrastructure.Identity;

public sealed class RedisSessionRevocationStore(
    IConnectionMultiplexer? redis,
    ILogger<RedisSessionRevocationStore> logger) : ISessionRevocationStore
{
    private const string KeyPrefix = "Hardware:revoked:sid:";

    public async Task RevokeAsync(string sessionId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (redis is null)
        {
            logger.LogDebug("Redis unavailable; session revocation skipped for {SessionId}", sessionId);
            return;
        }

        try
        {
            await redis.GetDatabase().StringSetAsync($"{KeyPrefix}{sessionId}", "1", ttl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to revoke session {SessionId} in Redis", sessionId);
        }
    }

    public async Task<bool> IsRevokedAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (redis is null) return false;

        try
        {
            return await redis.GetDatabase().KeyExistsAsync($"{KeyPrefix}{sessionId}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check session revocation for {SessionId}", sessionId);
            return false;
        }
    }
}
