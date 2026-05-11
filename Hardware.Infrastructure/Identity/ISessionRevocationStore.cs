namespace Hardware.Infrastructure.Identity;

public interface ISessionRevocationStore
{
    Task RevokeAsync(string sessionId, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> IsRevokedAsync(string sessionId, CancellationToken cancellationToken = default);
}
