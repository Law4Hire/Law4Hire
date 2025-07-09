using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(Law4HireDbContext context, ILogger? logger = null)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();
            logger?.LogInformation("Database ensured created");

            // Seed Service Packages
            if (!await context.ServicePackages.AnyAsync())
            {
                logger?.LogInformation("Seeding service packages");
                
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
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServicePackage
                    {
                        Name = "Hybrid Package with Attorney Overview",
                        Description = "Best of both worlds - handle your case with direct attorney oversight and review. Includes comprehensive legal guidance without G-28 filing, giving you more control while ensuring professional oversight.",
                        Type = PackageType.HybridWithAttorneyOverview,
                        BasePrice = 599.00m,
                        HasMoneyBackGuarantee = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServicePackage
                    {
                        Name = "Full Representation (Standard)",
                        Description = "Complete attorney representation from start to finish. Our experienced immigration attorneys handle your entire case, including G-28 filing, ensuring professional management of every detail.",
                        Type = PackageType.FullRepresentationStandard,
                        BasePrice = 1299.00m,
                        HasMoneyBackGuarantee = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServicePackage
                    {
                        Name = "Full Representation with Guarantee",
                        Description = "Our premium service with complete attorney representation, G-28 filing, and our exclusive money-back guarantee. If your case is not approved, we'll refund your attorney fees.",
                        Type = PackageType.FullRepresentationGuaranteed,
                        BasePrice = 1799.00m,
                        HasMoneyBackGuarantee = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.ServicePackages.AddRangeAsync(packages);
                logger?.LogInformation("Added {Count} service packages", packages.Length);
            }

            // Seed Sample Intake Questions
            if (!await context.IntakeQuestions.AnyAsync())
            {
                logger?.LogInformation("Seeding intake questions");
                
                var questions = new[]
                {
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "full_name",
                        QuestionText = "What is your full legal name as it appears on your passport or government-issued ID?",
                        Type = QuestionType.Text,
                        Order = 1,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"minLength\": 2, \"maxLength\": 100, \"pattern\": \"^[a-zA-Z\\\\s\\\\-\\\\.]+$\"}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "date_of_birth",
                        QuestionText = "What is your date of birth?",
                        Type = QuestionType.Date,
                        Order = 2,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"type\": \"date\", \"maxDate\": \"today\", \"minAge\": 16}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "country_of_birth",
                        QuestionText = "In which country were you born?",
                        Type = QuestionType.Text,
                        Order = 3,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"minLength\": 2, \"maxLength\": 50}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "current_immigration_status",
                        QuestionText = "What is your current immigration status in the United States?",
                        Type = QuestionType.MultipleChoice,
                        Order = 4,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Tourist/Visitor (B1/B2)\", \"Student (F1)\", \"Work Visa (H1B)\", \"Green Card Holder\", \"Citizen\", \"Asylum Seeker\", \"Other\", \"Undocumented\"]}",
                        Conditions = "{\"showIf\": {\"current_immigration_status\": [\"Tourist/Visitor (B1/B2)\", \"Undocumented\", \"Other\"]}}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "marriage_status",
                        QuestionText = "What is your current marital status?",
                        Type = QuestionType.MultipleChoice,
                        Order = 6,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Single\", \"Married\", \"Divorced\", \"Widowed\", \"Separated\"]}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "spouse_citizenship",
                        QuestionText = "What is your spouse's citizenship status?",
                        Type = QuestionType.MultipleChoice,
                        Order = 7,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"US Citizen\", \"Permanent Resident\", \"Foreign National\"]}",
                        Conditions = "{\"showIf\": {\"marriage_status\": [\"Married\"]}}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "employment_status",
                        QuestionText = "What is your current employment situation in the United States?",
                        Type = QuestionType.MultipleChoice,
                        Order = 8,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Employed with work authorization\", \"Employed without work authorization\", \"Self-employed\", \"Student\", \"Unemployed\", \"Retired\"]}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "legal_issues",
                        QuestionText = "Have you ever been arrested, charged with a crime, or had any legal issues in the US or any other country?",
                        Type = QuestionType.MultipleChoice,
                        Order = 9,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"No\", \"Yes - Minor traffic violations only\", \"Yes - Misdemeanor charges\", \"Yes - Felony charges\", \"Yes - Immigration violations\"]}"
                    },
                    new IntakeQuestion
                    {
                        Category = "Visit",
                        QuestionKey = "immigration_goal",
                        QuestionText = "What is your primary immigration goal?",
                        Type = QuestionType.MultipleChoice,
                       Order = 10,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Obtain Green Card\", \"Extend current visa\", \"Change visa status\", \"Apply for citizenship\", \"Bring family to US\", \"Work authorization\", \"Asylum/Protection\", \"Other\"]}"
                    }
                };

                await context.IntakeQuestions.AddRangeAsync(questions);
                logger?.LogInformation("Added {Count} intake questions", questions.Length);
            }

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
