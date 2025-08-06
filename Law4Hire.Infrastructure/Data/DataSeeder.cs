using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Logging;
using Law4Hire.Core.Enums;
// removed legacy VisaGroup enum import

namespace Law4Hire.Infrastructure.Data;

public static class DataSeeder
{
public static async Task SeedAsync(Law4HireDbContext context, ILogger? logger = null)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();
            logger?.LogInformation("Database ensured created");

            // Use the comprehensive seeder for Countries, US States, and Visa Types
            var comprehensiveSeeder = new SeedData.DataSeeder(context, 
                logger as ILogger<SeedData.DataSeeder> ?? 
                Microsoft.Extensions.Logging.Abstractions.NullLogger<SeedData.DataSeeder>.Instance);
            
            await comprehensiveSeeder.SeedAllDataAsync();

            // Seed Service Packages AFTER visa types exist
            if (!await context.ServicePackages.AnyAsync())
            {
                logger?.LogInformation("Seeding service packages");
                
                // Get a sample visa type to use as default
                var defaultVisaType = await context.BaseVisaTypes.FirstOrDefaultAsync();
                if (defaultVisaType == null)
                {
                    logger?.LogWarning("No visa types found, cannot seed service packages");
                    return;
                }

                var packages = new[]
                {
                    new ServicePackage
                    {
                        Name = "Self-Representation with Paralegal Overview",
                        Description = "Perfect for those who want to handle their case themselves with expert guidance. Our experienced paralegals will review your completed documents to ensure accuracy and completeness before submission.",
                        Type = PackageType.SelfRepresentationWithParalegal,
                        BasePrice = 299.00m,
                        HasMoneyBackGuarantee = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VisaTypeId = defaultVisaType.Id
                    },
                    new ServicePackage
                    {
                        Name = "Hybrid Package with Attorney Overview",
                        Description = "Best of both worlds - handle your case with direct attorney oversight and review. Includes comprehensive legal guidance without G-28 filing, giving you more control while ensuring professional oversight.",
                        Type = PackageType.HybridWithAttorneyOverview,
                        BasePrice = 599.00m,
                        HasMoneyBackGuarantee = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VisaTypeId = defaultVisaType.Id
                    },
                    new ServicePackage
                    {
                        Name = "Full Representation (Standard)",
                        Description = "Complete attorney representation from start to finish. Our experienced immigration attorneys handle your entire case, including G-28 filing, ensuring professional management of every detail.",
                        Type = PackageType.FullRepresentationStandard,
                        BasePrice = 1299.00m,
                        HasMoneyBackGuarantee = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VisaTypeId = defaultVisaType.Id
                    },
                    new ServicePackage
                    {
                        Name = "Full Representation with Guarantee",
                        Description = "Our premium service with complete attorney representation, G-28 filing, and our exclusive money-back guarantee. If your case is not approved, we'll refund your attorney fees.",
                        Type = PackageType.FullRepresentationGuaranteed,
                        BasePrice = 1799.00m,
                        HasMoneyBackGuarantee = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        VisaTypeId = defaultVisaType.Id
                    }
                };

                await context.ServicePackages.AddRangeAsync(packages);
                logger?.LogInformation("Added {Count} service packages", packages.Length);
            }

            // Seed Sample Intake Questions
            // Localized Content
            if (!await context.LocalizedContents.AnyAsync())
            {
                logger?.LogInformation("Seeding localized content");
                
                var localizedContent = new[]
                {
                    // English content
                    new LocalizedContent
                    {
                        ContentKey = "welcome_message",
                        Language = "en-US",
                        Content = "Welcome to Law4Hire! We're here to help you navigate your legal document needs with expert guidance and professional support.",
                        Description = "Welcome message for new users",
                        LastUpdated = DateTime.UtcNow
                    },
                    new LocalizedContent
                    {
                        ContentKey = "intake_intro",
                        Language = "en-US",
                        Content = "Our intelligent intake system will ask you a series of personalized questions to determine exactly what legal forms you need and gather all necessary information for your case.",
                        Description = "Introduction message for intake process",
                        LastUpdated = DateTime.UtcNow
                    },
                    new LocalizedContent
                    {
                        ContentKey = "privacy_notice",
                        Language = "en-US",
                        Content = "Your information is protected by attorney-client privilege and our strict privacy policy. All data is encrypted and securely stored.",
                        Description = "Privacy notice for users",
                        LastUpdated = DateTime.UtcNow
                    },
                    
                    // Spanish content
                    new LocalizedContent
                    {
                        ContentKey = "welcome_message",
                        Language = "es-ES",
                        Content = "¡Bienvenido a Law4Hire! Estamos aquí para ayudarte a navegar por tus necesidades de documentos legales con orientación experta y apoyo profesional.",
                        Description = "Welcome message for new users",
                        LastUpdated = DateTime.UtcNow
                    },
                    new LocalizedContent
                    {
                        ContentKey = "intake_intro",
                        Language = "es-ES",
                        Content = "Nuestro sistema inteligente de admisión te hará una serie de preguntas personalizadas para determinar exactamente qué formularios legales necesitas y recopilar toda la información necesaria para tu caso.",
                        Description = "Introduction message for intake process",
                        LastUpdated = DateTime.UtcNow
                    },
                    new LocalizedContent
                    {
                        ContentKey = "privacy_notice",
                        Language = "es-ES",
                        Content = "Tu información está protegida por el privilegio abogado-cliente y nuestra estricta política de privacidad. Todos los datos están cifrados y almacenados de forma segura.",
                        Description = "Privacy notice for users",
                        LastUpdated = DateTime.UtcNow
                    },

                    // French content
                    new LocalizedContent
                    {
                        ContentKey = "welcome_message",
                        Language = "fr-FR",
                        Content = "Bienvenue chez Law4Hire! Nous sommes là pour vous aider à naviguer dans vos besoins de documents juridiques con des conseils d'experts et un soutien professionnel.",
                        Description = "Welcome message for new users",
                        LastUpdated = DateTime.UtcNow
                    },
                    new LocalizedContent
                    {
                        ContentKey = "intake_intro",
                        Language = "fr-FR",
                        Content = "Notre système d'admission intelligent vous posera une serie de questions personnalisées pour déterminer exactement quels formulaires juridiques vous avez besoin et rassembler toutes les informations nécessaires pour votre cas.",
                        Description = "Introduction message for intake process",
                        LastUpdated = DateTime.UtcNow
                    },

                    // German content
                    new LocalizedContent
                    {
                        ContentKey = "welcome_message",
                        Language = "de-DE",
                        Content = "Willkommen bei Law4Hire! Wir sind hier, um Ihnen bei Ihren rechtlichen Dokumentenbedürfnissen mit fachkundiger Beratung und professioneller Unterstützung zu helfen.",
                        Description = "Welcome message for new users",
                        LastUpdated = DateTime.UtcNow
                    },

                    // Chinese content
                    new LocalizedContent
                    {
                        ContentKey = "welcome_message",
                        Language = "zh-CN",
                        Content = "欢迎来到Law4Hire！我们在这里通过专业指导和专业支持帮助您满足法律文件需求。",
                        Description = "Welcome message for new users",
                        LastUpdated = DateTime.UtcNow
                    }
                };

                await context.LocalizedContents.AddRangeAsync(localizedContent);
                logger?.LogInformation("Added {Count} localized content items", localizedContent.Length);
            }

            // Seed Visa Categories only (scraper-driven for types & docs)
            if (!await context.VisaCategories.AnyAsync())
            {
                logger?.LogInformation("Seeding visa categories");
                var categories = new[]
                {
                    new VisaCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Visit",      CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Immigrate", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Investment",CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Work",       CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Asylum",     CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Study",      CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new VisaCategory { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Family",     CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                await context.VisaCategories.AddRangeAsync(categories);
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }
}
