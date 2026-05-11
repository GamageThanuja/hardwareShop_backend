using Hardware.Domain.Common;
using Hardware.Domain.Entities.Identity;
using Hardware.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hardware.Infrastructure.Data.Interceptors;

public sealed class AuditInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = currentUser.UserId?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
            switch (entry.Entity)
            {
                case BaseEntity baseEntity:
                    ApplyToBase(entry, baseEntity, now, userId);
                    break;
                case ApplicationUser appUser:
                    ApplyToAppUser(entry, appUser, now);
                    break;
            }
    }

    private static void ApplyToBase(EntityEntry entry, BaseEntity entity, DateTime now, string? userId)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entity.CreatedAt = now;
                entity.IsDeleted = false;
                if (entity is AuditableEntity addedAuditable)
                    addedAuditable.CreatedBy = userId;
                break;

            case EntityState.Modified:
                entity.UpdatedAt = now;
                if (entity is AuditableEntity modifiedAuditable)
                    modifiedAuditable.UpdatedBy = userId;
                break;

            case EntityState.Deleted:
                // Convert hard delete to soft delete.
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.UpdatedAt = now;
                if (entity is AuditableEntity deletedAuditable)
                {
                    deletedAuditable.DeletedAt = now;
                    deletedAuditable.DeletedBy = userId;
                    deletedAuditable.UpdatedBy = userId;
                }

                break;
        }
    }

    private static void ApplyToAppUser(EntityEntry entry, ApplicationUser user, DateTime now)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                user.CreatedAt = new DateTimeOffset(now, TimeSpan.Zero);
                user.IsDeleted = false;
                break;
            case EntityState.Modified:
                user.UpdatedAt = now;
                break;
            case EntityState.Deleted:
                entry.State = EntityState.Modified;
                user.IsDeleted = true;
                user.UpdatedAt = now;
                break;
        }
    }
}
