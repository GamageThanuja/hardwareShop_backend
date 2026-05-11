using Hardware.Application.Common;
using Hardware.Application.DTOs.Auth;
using Hardware.Application.Services.Authentication;
using Hardware.Domain.Entities.Identity;
using Hardware.Infrastructure.Data;
using Hardware.Shared.Configuration;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hardware.Infrastructure.Identity;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext db,
    IJwtTokenGenerator jwt,
    IOptions<JwtSettings> jwtOptions,
    ISessionRevocationStore revocationStore,
    IDistributedCache cache,
    IHttpContextAccessor httpContext,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly TimeSpan FailedLoginCounterTtl = TimeSpan.FromMinutes(30);
    private readonly JwtSettings _jwt = jwtOptions.Value;

    // ---------- Register (admin-only at controller level) ----------

    public async Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!RoleConstants.All.Contains(dto.Role))
            return ApiResponse<TokenResponseDto>.Failure($"Invalid role '{dto.Role}'.");

        if (!await roleManager.RoleExistsAsync(dto.Role))
            return ApiResponse<TokenResponseDto>.Failure($"Role '{dto.Role}' is not configured.");

        if (await userManager.FindByEmailAsync(dto.Email) is not null)
            return ApiResponse<TokenResponseDto>.Failure("Email already registered.", ["EMAIL_TAKEN"]);

        if (await userManager.FindByNameAsync(dto.UserName) is not null)
            return ApiResponse<TokenResponseDto>.Failure("Username already taken.", ["USERNAME_TAKEN"]);

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true,
            PhoneNumber = dto.PhoneNumber,
            PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(dto.PhoneNumber),
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var create = await userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            var errors = create.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("User creation failed for {UserName}: {Errors}", dto.UserName, string.Join(", ", errors));
            return ApiResponse<TokenResponseDto>.Failure("Registration failed.", errors);
        }

        var addRole = await userManager.AddToRoleAsync(user, dto.Role);
        if (!addRole.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ApiResponse<TokenResponseDto>.Failure("Failed to assign role.",
                addRole.Errors.Select(e => e.Description).ToList());
        }

        var tokens = await IssueNewSessionAsync(user, [dto.Role], cancellationToken);
        logger.LogInformation("Registered user {UserId} with role {Role}", user.Id, dto.Role);
        return ApiResponse<TokenResponseDto>.Success(tokens, "Registered.");
    }

    // ---------- Login ----------

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var emailKey = dto.UserName.Trim().ToLowerInvariant();
        var failedKey = $"auth:failed_login:{emailKey}";

        if (await GetFailedAttemptsAsync(failedKey, cancellationToken) >= _jwt.MaxFailedLoginAttempts)
        {
            logger.LogWarning("Account locked for {Email} (too many failed attempts)", emailKey);
            return ApiResponse<TokenResponseDto>.Failure("Account temporarily locked.", ["ACCOUNT_LOCKED"]);
        }

        var user = await FindUserAsync(dto.UserName, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            await IncrementFailedAttemptsAsync(failedKey, cancellationToken);
            return ApiResponse<TokenResponseDto>.Failure("Invalid credentials.", ["INVALID_CREDENTIALS"]);
        }

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            await IncrementFailedAttemptsAsync(failedKey, cancellationToken);
            return ApiResponse<TokenResponseDto>.Failure("Invalid credentials.", ["INVALID_CREDENTIALS"]);
        }

        await cache.RemoveAsync(failedKey, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await EnforceSessionCapAsync(user.Id, cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var tokens = await IssueNewSessionAsync(user, roles, cancellationToken);
        logger.LogInformation("User {UserId} logged in (sid={SessionId})", user.Id, tokens.SessionId);
        return ApiResponse<TokenResponseDto>.Success(tokens, "Logged in.");
    }

    // ---------- Refresh ----------

    public async Task<ApiResponse<TokenResponseDto>> RefreshAsync(RefreshTokenDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return ApiResponse<TokenResponseDto>.Failure("Refresh token required.", ["INVALID_TOKEN"]);

        var hash = RefreshTokenHasher.Hash(dto.RefreshToken);
        var existing = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null)
            return ApiResponse<TokenResponseDto>.Failure("Invalid refresh token.", ["INVALID_TOKEN"]);

        if (!existing.IsActive)
        {
            // Reuse detected — revoke entire family.
            logger.LogWarning(
                "Refresh token reuse detected. UserId={UserId} FamilyId={FamilyId} SessionId={SessionId} Ip={Ip}",
                existing.UserId, existing.FamilyId, existing.SessionId,
                httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "?");

            await RevokeFamilyAsync(existing.FamilyId, "reuse-detected", cancellationToken);
            return ApiResponse<TokenResponseDto>.Failure(
                "Token reuse detected; all sessions revoked.", ["TOKEN_REUSE"]);
        }

        var user = existing.User;
        if (user.IsDeleted)
            return ApiResponse<TokenResponseDto>.Failure("Account is no longer active.", ["ACCOUNT_INACTIVE"]);

        var (rawToken, tokenHash) = RefreshTokenHasher.Create();
        var device = httpContext.HttpContext.ToDeviceInfo();
        var now = DateTime.UtcNow;
        var slidingExpiry = now.AddDays(_jwt.RefreshTokenExpirationDays);
        var newExpiresUtc = slidingExpiry < existing.AbsoluteExpiresUtc ? slidingExpiry : existing.AbsoluteExpiresUtc;

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            FamilyId = existing.FamilyId,
            SessionId = existing.SessionId,
            DeviceId = device.DeviceId ?? existing.DeviceId,
            DeviceName = device.DeviceName ?? existing.DeviceName,
            UserAgent = device.UserAgent ?? existing.UserAgent,
            IpAddress = device.IpAddress ?? existing.IpAddress,
            ClientType = device.ClientType ?? existing.ClientType,
            CreatedUtc = now,
            ExpiresUtc = newExpiresUtc,
            AbsoluteExpiresUtc = existing.AbsoluteExpiresUtc,
            LastUsedUtc = null
        };

        existing.RevokedUtc = now;
        existing.RevokedReason = "rotated";
        existing.LastUsedUtc = now;
        existing.ReplacedByTokenId = newToken.Id;

        db.RefreshTokens.Add(newToken);
        await db.SaveChangesAsync(cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var access = jwt.GenerateAccessToken(user, roles, existing.SessionId);

        var response = new TokenResponseDto(
            access.Token,
            rawToken,
            access.ExpiresAt,
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles.ToList(),
            existing.SessionId);

        return ApiResponse<TokenResponseDto>.Success(response, "Refreshed.");
    }

    // ---------- Logout (single session) ----------

    public async Task<ApiResponse<object>> LogoutAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ApiResponse.Success("Already logged out.");

        var hash = RefreshTokenHasher.Hash(refreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (token is null) return ApiResponse.Success("Already logged out.");

        if (token.RevokedUtc is null)
        {
            token.RevokedUtc = DateTime.UtcNow;
            token.RevokedReason = "logout-session";
            await db.SaveChangesAsync(cancellationToken);
        }

        await revocationStore.RevokeAsync(
            token.SessionId,
            TimeSpan.FromMinutes(_jwt.AccessTokenExpirationMinutes + 1),
            cancellationToken);

        return ApiResponse.Success("Logged out.");
    }

    // ---------- Logout all sessions ----------

    public async Task<ApiResponse<object>> LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessionIds = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null)
            .Select(t => t.SessionId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedUtc, now)
                .SetProperty(t => t.RevokedReason, "logout-all"), cancellationToken);

        var ttl = TimeSpan.FromMinutes(_jwt.AccessTokenExpirationMinutes + 1);
        foreach (var sid in sessionIds.Distinct())
            await revocationStore.RevokeAsync(sid, ttl, cancellationToken);

        return ApiResponse.Success("All sessions logged out.");
    }

    // ---------- List sessions for /me/sessions ----------

    public async Task<IReadOnlyList<SessionSummaryDto>> ListSessionsAsync(
        Guid userId, string? currentSessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var rows = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null && t.ExpiresUtc > now)
            .OrderByDescending(t => t.LastUsedUtc ?? t.CreatedUtc)
            .Select(t => new
            {
                t.SessionId,
                t.DeviceName,
                t.ClientType,
                t.IpAddress,
                t.CreatedUtc,
                t.LastUsedUtc
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.SessionId)
            .Select(g =>
            {
                var newest = g.First();
                return new SessionSummaryDto(
                    newest.SessionId,
                    newest.DeviceName,
                    newest.ClientType,
                    newest.IpAddress,
                    newest.CreatedUtc,
                    newest.LastUsedUtc,
                    newest.SessionId == currentSessionId);
            })
            .ToList();
    }

    public async Task<ApiResponse<object>> RevokeSessionAsync(
        Guid userId, string sessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var affected = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.SessionId == sessionId && t.RevokedUtc == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedUtc, now)
                .SetProperty(t => t.RevokedReason, "logout-session"), cancellationToken);

        if (affected > 0)
            await revocationStore.RevokeAsync(
                sessionId,
                TimeSpan.FromMinutes(_jwt.AccessTokenExpirationMinutes + 1),
                cancellationToken);

        return ApiResponse.Success("Session revoked.");
    }

    // ---------- helpers ----------

    private async Task<TokenResponseDto> IssueNewSessionAsync(
        ApplicationUser user, IList<string> roles, CancellationToken cancellationToken)
    {
        var sessionId = jwt.GenerateSessionId();
        var familyId = Guid.NewGuid();
        var (rawToken, tokenHash) = RefreshTokenHasher.Create();
        var device = httpContext.HttpContext.ToDeviceInfo();
        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            FamilyId = familyId,
            SessionId = sessionId,
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            UserAgent = device.UserAgent,
            IpAddress = device.IpAddress,
            ClientType = device.ClientType,
            CreatedUtc = now,
            ExpiresUtc = now.AddDays(_jwt.RefreshTokenExpirationDays),
            AbsoluteExpiresUtc = now.AddDays(_jwt.RefreshTokenAbsoluteLifetimeDays)
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var access = jwt.GenerateAccessToken(user, roles, sessionId);

        return new TokenResponseDto(
            access.Token,
            rawToken,
            access.ExpiresAt,
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles.ToList(),
            sessionId);
    }

    private async Task EnforceSessionCapAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null && t.ExpiresUtc > now)
            .OrderBy(t => t.LastUsedUtc ?? t.CreatedUtc)
            .ToListAsync(cancellationToken);

        var overflow = active.Count - (_jwt.MaxActiveSessionsPerUser - 1);
        if (overflow <= 0) return;

        foreach (var token in active.Take(overflow))
        {
            token.RevokedUtc = now;
            token.RevokedReason = "session-cap";
            await revocationStore.RevokeAsync(
                token.SessionId,
                TimeSpan.FromMinutes(_jwt.AccessTokenExpirationMinutes + 1),
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken cancellationToken) =>
        db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedUtc == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedUtc, DateTime.UtcNow)
                .SetProperty(t => t.RevokedReason, reason), cancellationToken);

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail, CancellationToken cancellationToken)
    {
        // Allow login by email or username.
        var byEmail = await userManager.FindByEmailAsync(userNameOrEmail);
        if (byEmail is not null) return byEmail;
        return await userManager.FindByNameAsync(userNameOrEmail);
    }

    private async Task<int> GetFailedAttemptsAsync(string key, CancellationToken cancellationToken)
    {
        var raw = await cache.GetStringAsync(key, cancellationToken);
        return int.TryParse(raw, out var n) ? n : 0;
    }

    private async Task IncrementFailedAttemptsAsync(string key, CancellationToken cancellationToken)
    {
        var current = await GetFailedAttemptsAsync(key, cancellationToken);
        await cache.SetStringAsync(
            key,
            (current + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = FailedLoginCounterTtl },
            cancellationToken);
    }
}
