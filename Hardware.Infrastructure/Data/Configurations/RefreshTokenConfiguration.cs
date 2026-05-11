using Hardware.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(t => t.Id);

        b.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
        b.Property(t => t.SessionId).IsRequired().HasMaxLength(32);
        b.Property(t => t.DeviceId).HasMaxLength(128);
        b.Property(t => t.DeviceName).HasMaxLength(256);
        b.Property(t => t.UserAgent).HasMaxLength(512);
        b.Property(t => t.IpAddress).HasMaxLength(64);
        b.Property(t => t.ClientType).HasMaxLength(32);
        b.Property(t => t.RevokedReason).HasMaxLength(128);

        b.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("IX_RefreshTokens_TokenHash");
        b.HasIndex(t => new { t.UserId, t.RevokedUtc }).HasDatabaseName("IX_RefreshTokens_UserId_RevokedUtc");
        b.HasIndex(t => t.FamilyId).HasDatabaseName("IX_RefreshTokens_FamilyId");
        b.HasIndex(t => t.ExpiresUtc).HasDatabaseName("IX_RefreshTokens_ExpiresUtc");
        b.HasIndex(t => t.SessionId).HasDatabaseName("IX_RefreshTokens_SessionId");

        b.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Ignore(t => t.IsActive);
    }
}
