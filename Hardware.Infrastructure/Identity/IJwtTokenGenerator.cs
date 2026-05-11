using Hardware.Domain.Entities.Identity;

namespace Hardware.Infrastructure.Identity;

public sealed record GeneratedAccessToken(string Token, DateTime ExpiresAt, string SessionId);

public interface IJwtTokenGenerator
{
    GeneratedAccessToken GenerateAccessToken(ApplicationUser user, IList<string> roles, string sessionId);
    string GenerateSessionId();
}
