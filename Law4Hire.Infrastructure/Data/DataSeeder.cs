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

            // Visa Groups and Types
            if (!await context.VisaTypes.AnyAsync())
            {
                logger?.LogInformation("Seeding visa types and related data");

                var visitGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var immigrateGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var investmentGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
                var workGroupId = Guid.Parse("44444444-4444-4444-4444-444444444444");
                var asylumGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555");
                var studyGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666");
                var familyGroupId = Guid.Parse("77777777-7777-7777-7777-777777777777");

                var visaGroups = new[]
                {
                    new VisaGroup { Id = visitGroupId, Name = VisaGroupEnum.Visit },
                    new VisaGroup { Id = immigrateGroupId, Name = VisaGroupEnum.Immigrate },
                    new VisaGroup { Id = investmentGroupId, Name = VisaGroupEnum.Investment },
                    new VisaGroup { Id = workGroupId, Name = VisaGroupEnum.Work },
                    new VisaGroup { Id = asylumGroupId, Name = VisaGroupEnum.Asylum },
                    new VisaGroup { Id = studyGroupId, Name = VisaGroupEnum.Study },
                    new VisaGroup { Id = familyGroupId, Name = VisaGroupEnum.Family }
                };

                await context.VisaGroups.AddRangeAsync(visaGroups);
                logger?.LogInformation("Added {Count} visa groups", visaGroups.Length);

                var b1b2Id = Guid.Parse("1aa2bf5e-7242-49c2-9303-04ddef44e8b1");
                var gcFamilyId = Guid.Parse("a8e01e04-27a1-4380-b9cc-cace62830fab");
                var f1Id = Guid.Parse("cdeb31d4-d778-495b-a4e3-dcd67e1aa737");
                var h1bId = Guid.Parse("162e3e30-ec8b-438e-8f96-e836465d0908");
                var asylumId = Guid.Parse("a767bd9a-272e-440d-99fc-955e1b1d9303");

                var visaTypes = new[]
                {
                    new VisaType
                    {
                        Id = b1b2Id,
                        Name = "B1/B2 Visitor Visa",
                        Description = "Temporary visit for business or tourism",
                        Category = "Visit",
                        VisaGroupId = visitGroupId
                    },
                    new VisaType
                    {
                        Id = gcFamilyId,
                        Name = "Family Based Green Card",
                        Description = "Immigrate through qualifying family",
                        Category = "Immigrate",
                        VisaGroupId = immigrateGroupId
                    },
                    new VisaType
                    {
                        Id = f1Id,
                        Name = "F1 Student Visa",
                        Description = "Academic study in the U.S.",
                        Category = "Study",
                        VisaGroupId = studyGroupId
                    },
                    new VisaType
                    {
                        Id = h1bId,
                        Name = "H1B Specialty Occupation",
                        Description = "Work visa for specialty occupations",
                        Category = "Work",
                        VisaGroupId = workGroupId
                    },
                    new VisaType
                    {
                        Id = asylumId,
                        Name = "Asylum",
                        Description = "Protection for those fearing persecution",
                        Category = "Protect",
                        VisaGroupId = asylumGroupId
                    },
                    new VisaType
                    {
                        Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        Name = "EB-5 Immigrant Investor",
                        Description = "Investment-based path to permanent residence",
                        Category = "Investment",
                        VisaGroupId = investmentGroupId
                    },
                    new VisaType
                    {
                        Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        Name = "K-1 Fianc\u00E9(e) Visa",
                        Description = "For foreign-citizen fianc\u00E9s of U.S. citizens",
                        Category = "Family",
                        VisaGroupId = familyGroupId
                    }
                };

                await context.VisaTypes.AddRangeAsync(visaTypes);
                logger?.LogInformation("Added {Count} visa types", visaTypes.Length);

                var i130Id = Guid.Parse("2919140a-6230-42a7-a7c8-6972b8a19311");
                var i485Id = Guid.Parse("411dfd9e-6180-49ab-80b8-3756541de505");
                var i864Id = Guid.Parse("fe86ca4b-3808-482b-89fb-e2fc9375684b");
                var ds160Id = Guid.Parse("678634b3-de95-4f16-83c9-dc86aad68723");
                var i129Id = Guid.Parse("3e27b6a3-8a52-4185-854c-5f584aea28e8");

                var documents = new[]
                {
                    new DocumentType
                    {
                        Id = i130Id,
                        FormNumber = "I-130",
                        Name = "Petition for Alien Relative",
                        Description = "Establish qualifying family relationship",
                        IssuingAgency = "USCIS"
                    },
                    new DocumentType
                    {
                        Id = i485Id,
                        FormNumber = "I-485",
                        Name = "Application to Register Permanent Residence",
                        Description = "Apply for permanent residence",
                        IssuingAgency = "USCIS"
                    },
                    new DocumentType
                    {
                        Id = i864Id,
                        FormNumber = "I-864",
                        Name = "Affidavit of Support",
                        Description = "Sponsor financial support form",
                        IssuingAgency = "USCIS"
                    },
                    new DocumentType
                    {
                        Id = ds160Id,
                        FormNumber = "DS-160",
                        Name = "Online Nonimmigrant Visa Application",
                        Description = "Application for temporary visas",
                        IssuingAgency = "DOS"
                    },
                    new DocumentType
                    {
                        Id = i129Id,
                        FormNumber = "I-129",
                        Name = "Petition for a Nonimmigrant Worker",
                        Description = "For H-1B and other workers",
                        IssuingAgency = "USCIS"
                    }
                };

                await context.DocumentTypes.AddRangeAsync(documents);
                logger?.LogInformation("Added {Count} document types", documents.Length);

                var requirements = new[]
                {
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("77ff7722-759d-4906-8f2b-d3e393ed9587"),
                        VisaTypeId = b1b2Id,
                        DocumentTypeId = ds160Id,
                        IsRequired = true
                    },
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("3b7bb132-1a0c-4d20-9c31-f4d5fd31bf6a"),
                        VisaTypeId = gcFamilyId,
                        DocumentTypeId = i130Id,
                        IsRequired = true
                    },
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("a33e806d-6d34-4f2b-b1b9-0ceaaf479faf"),
                        VisaTypeId = gcFamilyId,
                        DocumentTypeId = i485Id,
                        IsRequired = true
                    },
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("abb2b7e9-0fb2-4f56-b96e-a1806f6df7f8"),
                        VisaTypeId = gcFamilyId,
                        DocumentTypeId = i864Id,
                        IsRequired = true
                    },
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("c9ea5edd-974b-4150-bee5-2cc506cf9ac9"),
                        VisaTypeId = f1Id,
                        DocumentTypeId = ds160Id,
                        IsRequired = true
                    },
                    new VisaDocumentRequirement
                    {
                        Id = Guid.Parse("25d18774-32bd-466f-9b6e-677d23dbe0de"),
                        VisaTypeId = h1bId,
                        DocumentTypeId = i129Id,
                        IsRequired = true
                    }
                };

                await context.VisaDocumentRequirements.AddRangeAsync(requirements);
                logger?.LogInformation("Added {Count} visa document requirements", requirements.Length);
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
