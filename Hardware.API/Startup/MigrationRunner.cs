using Hardware.Domain.Entities.Identity;
using Hardware.Infrastructure.Data;
using Hardware.Shared.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hardware.API.Startup;

public static class MigrationRunner
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<WebApplication>>();
        try
        {
            var ctx = sp.GetRequiredService<ApplicationDbContext>();
            await ctx.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var seedData = sp.GetRequiredService<IOptions<SeedDataSettings>>().Value;

            await ApplicationDbContextSeed.SeedAsync(userManager, roleManager, seedData, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration / seeding failed");
            throw;
        }
    }
}
