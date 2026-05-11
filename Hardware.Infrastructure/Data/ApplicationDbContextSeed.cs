using System.ComponentModel.DataAnnotations;
using Hardware.Domain.Entities.Identity;
using Hardware.Shared.Configuration;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hardware.Infrastructure.Data;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        SeedDataSettings seedData,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(roleManager, logger);
        await SeedAdminAsync(userManager, seedData, logger);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger logger)
    {
        foreach (var role in RoleConstants.All)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (result.Succeeded)
                logger.LogInformation("Seeded role {Role}", role);
            else
                logger.LogWarning("Failed to seed role {Role}: {Errors}", role,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        SeedDataSettings seedData,
        ILogger logger)
    {
        if (!seedData.EnableSeeding)
        {
            logger.LogInformation("Admin seeding disabled (SeedData:EnableSeeding=false)");
            return;
        }

        if (!ValidateSeedSettings(seedData, logger)) return;

        var existing = await userManager.FindByEmailAsync(seedData.AdminEmail);
        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                logger.LogWarning(
                    "Admin user {Email} exists but is soft-deleted; not auto-restored. Restore manually if intentional.",
                    seedData.AdminEmail);
                return;
            }

            // Idempotent: ensure Admin role assignment.
            if (!await userManager.IsInRoleAsync(existing, RoleConstants.Admin))
            {
                var addRoleResult = await userManager.AddToRoleAsync(existing, RoleConstants.Admin);
                if (addRoleResult.Succeeded)
                    logger.LogInformation("Existing user {Email} promoted to Admin", seedData.AdminEmail);
                else
                    logger.LogWarning("Failed to promote {Email} to Admin: {Errors}", seedData.AdminEmail,
                        string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            }
            else
            {
                logger.LogInformation("Admin user {Email} already exists; seed skipped", seedData.AdminEmail);
            }

            return;
        }

        var admin = new ApplicationUser
        {
            UserName = seedData.AdminUserName,
            Email = seedData.AdminEmail,
            EmailConfirmed = true,
            PhoneNumber = seedData.AdminPhoneNumber,
            PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(seedData.AdminPhoneNumber),
            FirstName = seedData.AdminFirstName,
            LastName = seedData.AdminLastName
        };

        var create = await userManager.CreateAsync(admin, seedData.AdminPassword);
        if (!create.Succeeded)
        {
            logger.LogError("Failed to seed admin user {Email}: {Errors}", seedData.AdminEmail,
                string.Join(", ", create.Errors.Select(e => e.Description)));
            return;
        }

        var addRole = await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
        if (!addRole.Succeeded)
        {
            logger.LogError("Admin user {Email} created but failed to assign Admin role: {Errors}",
                seedData.AdminEmail,
                string.Join(", ", addRole.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation(
            "Admin user seeded: Email={Email}, UserName={UserName}, Id={UserId}",
            admin.Email, admin.UserName, admin.Id);

        WarnIfDefaultPassword(seedData, logger);
    }

    private static bool ValidateSeedSettings(SeedDataSettings seedData, ILogger logger)
    {
        var ctx = new ValidationContext(seedData);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(seedData, ctx, results, true))
            return true;

        foreach (var error in results)
            logger.LogError("SeedData configuration invalid: {Field} - {Message}",
                string.Join(", ", error.MemberNames), error.ErrorMessage);
        logger.LogError(
            "Admin seeding skipped due to invalid SeedData configuration. Fix appsettings or set EnableSeeding=false.");
        return false;
    }

    private static void WarnIfDefaultPassword(SeedDataSettings seedData, ILogger logger)
    {
        var weakIndicators = new[] { "admin", "password", "changeme", "123456", "kumudu" };
        var pwd = seedData.AdminPassword.ToLowerInvariant();
        if (weakIndicators.Any(w => pwd.Contains(w)))
            logger.LogWarning(
                "Seeded admin password appears weak/default. Change it immediately and disable seeding (SeedData:EnableSeeding=false) in production.");
    }
}
