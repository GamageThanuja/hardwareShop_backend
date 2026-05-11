using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hardware.Domain.Entities.Identity;
using Hardware.Shared.Configuration;
using Hardware.Shared.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Hardware.Infrastructure.Identity;

public sealed class JwtTokenGenerator(IOptions<JwtSettings> options) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public GeneratedAccessToken GenerateAccessToken(ApplicationUser user, IList<string> roles, string sessionId)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(CustomClaimTypes.FullName, $"{user.FirstName} {user.LastName}".Trim()),
            new(CustomClaimTypes.EmailVerified, user.EmailConfirmed.ToString()),
            new(CustomClaimTypes.PhoneVerified, user.PhoneNumberConfirmed.ToString()),
            new(CustomClaimTypes.SessionId, sessionId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecurityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            DateTime.UtcNow,
            expires,
            creds);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return new GeneratedAccessToken(encoded, expires, sessionId);
    }

    public string GenerateSessionId() => Guid.NewGuid().ToString("N");
}
