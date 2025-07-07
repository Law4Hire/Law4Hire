using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Law4Hire.Infrastructure.Data.Initialization;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<Law4HireDbContext>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = ["Admin", "AI", "LegalProfessional"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed AI user
        var aiEmail = "ai@law4hire.com";
        var aiUser = await userManager.FindByEmailAsync(aiEmail);
        if (aiUser == null)
        {
            aiUser = new User
            {
                UserName = aiEmail,
                Email = aiEmail,
                FirstName = "Law4Hire",
                LastName = "Agent",
                PhoneNumber = "+1-555-000-0000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(aiUser, "SuperSecure!123");
            await userManager.AddToRoleAsync(aiUser, "AI");
        }

        // Seed LegalProfessional user (Denise Cann)
        var deniseEmail = "denise.cann@cannlaw.com";
        var deniseUser = await userManager.FindByEmailAsync(deniseEmail);
        if (deniseUser == null)
        {
            deniseUser = new User
            {
                UserName = deniseEmail,
                Email = deniseEmail,
                FirstName = "Denise",
                LastName = "Cann",
                PhoneNumber = "+1-555-111-2222",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(deniseUser, "Law4HireSecure!");
            await userManager.AddToRoleAsync(deniseUser, "LegalProfessional");

            context.LegalProfessionals.Add(new LegalProfessional
            {
                Id = deniseUser.Id,
                BarNumber = "CN123456"
            });

            await context.SaveChangesAsync();
        }

        // You can extend this section to seed visa types, document types, etc.
    }
}
