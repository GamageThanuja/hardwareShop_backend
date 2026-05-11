using Hardware.Application.Common;
using Hardware.Application.DTOs.Auth;

namespace Hardware.Application.Services.Authentication;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<TokenResponseDto>>
        RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionSummaryDto>> ListSessionsAsync(Guid userId, string? currentSessionId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> RevokeSessionAsync(Guid userId, string sessionId,
        CancellationToken cancellationToken = default);
}
