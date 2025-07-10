using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Law4Hire.Infrastructure.Data.Initialization;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<Law4HireDbContext>();

        await context.Database.MigrateAsync();

        // Seed AI user
        var aiEmail = "ai@law4hire.com";
        if (!await context.Users.AnyAsync(u => u.Email == aiEmail))
        {
            CreatePasswordHash("SuperSecure!123", out var hash, out var salt);
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
        var deniseEmail = "dcann@cannlaw.com";
        if (!await context.Users.AnyAsync(u => u.Email == deniseEmail))
        {
            CreatePasswordHash("Law4HireSecure!", out var hash, out var salt);
            var deniseUser = new User
            {
                UserName = deniseEmail,
                Email = deniseEmail,
                FirstName = "Denise",
                LastName = "Cann",
                PhoneNumber = "+1-410-783-1888",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            context.Users.Add(deniseUser);
            context.LegalProfessionals.Add(new LegalProfessional
            {
                Id = deniseUser.Id,
                BarNumber = "1004384"
            });

            await context.SaveChangesAsync();
        }

        // You can extend this section to seed visa types, document types, etc.
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
