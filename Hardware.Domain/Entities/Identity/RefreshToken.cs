namespace Hardware.Domain.Entities.Identity;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public string SessionId { get; set; } = string.Empty;

    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? ClientType { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime AbsoluteExpiresUtc { get; set; }
    public DateTime? LastUsedUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public string? RevokedReason { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public bool IsActive =>
        RevokedUtc is null
        && DateTime.UtcNow < ExpiresUtc
        && DateTime.UtcNow < AbsoluteExpiresUtc;
}
