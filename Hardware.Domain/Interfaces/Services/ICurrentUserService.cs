namespace Hardware.Domain.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
