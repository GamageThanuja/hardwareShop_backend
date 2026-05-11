namespace Hardware.Application.Services.Identity;

public interface IIdentityService
{
    Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
