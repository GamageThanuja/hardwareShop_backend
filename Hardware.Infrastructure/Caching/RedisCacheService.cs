using System.Text.Json;
using Hardware.Shared.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Hardware.Infrastructure.Caching;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IMemoryCache memoryCache,
    IConnectionMultiplexer? connectionMultiplexer,
    IOptions<RedisSettings> redisOptions,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(redisOptions.Value.DefaultExpirationMinutes);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await distributedCache.GetStringAsync(key, cancellationToken);
            return string.IsNullOrEmpty(raw) ? default : JsonSerializer.Deserialize<T>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis GET failed for {Key}; falling back to memory cache", key);
            return memoryCache.TryGetValue<T>(key, out var value) ? value : default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var ttl = expiration ?? _defaultExpiration;
        var json = JsonSerializer.Serialize(value, JsonOptions);
        try
        {
            await distributedCache.SetStringAsync(key, json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis SET failed for {Key}; storing in memory cache instead", key);
            memoryCache.Set(key, value, ttl);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing is not null) return existing;

        var fresh = await factory(cancellationToken);
        if (fresh is not null) await SetAsync(key, fresh, expiration, cancellationToken);
        return fresh;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis REMOVE failed for {Key}", key);
        }

        memoryCache.Remove(key);
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (connectionMultiplexer is null)
        {
            logger.LogDebug("Pattern removal skipped: Redis multiplexer unavailable");
            return Task.CompletedTask;
        }

        try
        {
            var endpoints = connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = connectionMultiplexer.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern);
                var db = connectionMultiplexer.GetDatabase();
                foreach (var key in keys) db.KeyDelete(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis pattern removal failed for {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }
}
