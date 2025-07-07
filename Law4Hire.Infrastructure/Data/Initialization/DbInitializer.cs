using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Law4Hire.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Law4Hire.Infrastructure.Data.Initialization;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<Law4HireDbContext>();
        var authService = serviceProvider.GetRequiredService<IAuthService>();

        await context.Database.MigrateAsync();

        // Seed AI user
        var aiEmail = "ai@law4hire.com";
        if (!await context.Users.AnyAsync(u => u.Email == aiEmail))
        {
            authService.CreatePasswordHash("SuperSecure!123", out var hash, out var salt);
            var aiUser = new User
            {
                UserName = aiEmail,
                Email = aiEmail,
                FirstName = "Law4Hire",
                LastName = "Agent",
                PhoneNumber = "+1-555-000-0000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            context.Users.Add(aiUser);
            await context.SaveChangesAsync();
        }

        // Seed LegalProfessional user (Denise Cann)
        var deniseEmail = "denise.cann@cannlaw.com";
        if (!await context.Users.AnyAsync(u => u.Email == deniseEmail))
        {
            authService.CreatePasswordHash("Law4HireSecure!", out var hash, out var salt);
            var deniseUser = new User
            {
                UserName = deniseEmail,
                Email = deniseEmail,
                FirstName = "Denise",
                LastName = "Cann",
                PhoneNumber = "+1-555-111-2222",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            context.Users.Add(deniseUser);
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
