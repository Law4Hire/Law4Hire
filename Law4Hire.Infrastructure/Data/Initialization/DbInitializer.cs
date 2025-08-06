using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Infrastructure.Data.Initialization;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<Law4HireDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("DbInitializer");

        await context.Database.MigrateAsync();

        // Ensure roles exist
        await EnsureRolesExistAsync(roleManager, logger);

        // Seed admin users
        await SeedAdminUsersAsync(context, userManager, logger);
    }

    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger? logger)
    {
        var roles = new[] { "Admin", "LegalProfessionals" };
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>(roleName) { Id = Guid.NewGuid() };
                await roleManager.CreateAsync(role);
                logger?.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }

    private static async Task SeedAdminUsersAsync(Law4HireDbContext context, UserManager<User> userManager, ILogger? logger)
    {
        // Seed AI user
        var aiEmail = "ai@law4hire.com";
        if (!await context.Users.AnyAsync(u => u.Email == aiEmail))
        {
            var aiUser = new User
            {
                Id = Guid.NewGuid(),
                Email = aiEmail,
                UserName = aiEmail,
                EmailConfirmed = true,
                FirstName = "AI",
                LastName = "Assistant",
                PhoneNumber = "+1-555-0100",
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Category = "Admin"
            };

            var result = await userManager.CreateAsync(aiUser, "SecureTest123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(aiUser, "Admin");
                await userManager.AddToRoleAsync(aiUser, "LegalProfessionals");
                logger?.LogInformation("Created AI admin user: {Email}", aiEmail);
            }
            else
            {
                logger?.LogError("Failed to create AI user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed LegalProfessional user (Denise Cann)  
        var deniseEmail = "dcann@cannlaw.com";
        if (!await context.Users.AnyAsync(u => u.Email == deniseEmail))
        {
            var deniseUser = new User
            {
                Id = Guid.NewGuid(),
                Email = deniseEmail,
                UserName = deniseEmail,
                EmailConfirmed = true,
                FirstName = "Denise",
                LastName = "Cann",
                PhoneNumber = "+1-555-0101",
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Category = "LegalProfessional"
            };

            var result = await userManager.CreateAsync(deniseUser, "SecureTest123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(deniseUser, "Admin");
                await userManager.AddToRoleAsync(deniseUser, "LegalProfessionals");
                logger?.LogInformation("Created Denise admin user: {Email}", deniseEmail);
            }
            else
            {
                logger?.LogError("Failed to create Denise user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed test user (Abu)
        var abuEmail = "Abu@testing.com";
        if (!await context.Users.AnyAsync(u => u.Email == abuEmail))
        {
            var abuUser = new User
            {
                Id = Guid.NewGuid(),
                Email = abuEmail,
                UserName = "abutesting",
                EmailConfirmed = true,
                FirstName = "Abu",
                LastName = "Testing",
                PhoneNumber = "+1-555-0102",
                PreferredLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Category = "Client"
            };

            var result = await userManager.CreateAsync(abuUser, "TestPassword123!");
            if (result.Succeeded)
            {
                logger?.LogInformation("Created test user: {Email}", abuEmail);
            }
            else
            {
                logger?.LogError("Failed to create Abu test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
