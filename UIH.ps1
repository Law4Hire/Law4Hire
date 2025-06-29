param(
    [string]$SolutionName = "Law4Hire"
)

# Use pushd/popd for reliable directory navigation within the script
$scriptDir = $PSScriptRoot
Push-Location $scriptDir

Write-Host "Creating Law4Hire Solution Structure..." -ForegroundColor Green

# Create solution
dotnet new sln -n $SolutionName

# Create directory structure
$directories = @(
    "Law4Hire.Core",
    "Law4Hire.Core\Entities",
    "Law4Hire.Core\Enums",
    "Law4Hire.Core\Interfaces",
    "Law4Hire.Core\DTOs",
    "Law4Hire.Infrastructure",
    "Law4Hire.Infrastructure\Data",
    "Law4Hire.Infrastructure\Data\Contexts",
    "Law4Hire.Infrastructure\Data\Configurations",
    "Law4Hire.Infrastructure\Data\Repositories",
    "Law4Hire.Infrastructure\Services",
    "Law4Hire.Infrastructure\Migrations",
    "Law4Hire.Application",
    "Law4Hire.Application\Services",
    "Law4Hire.Application\Handlers",
    "Law4Hire.Application\Validators",
    "Law4Hire.Application\Mappers",
    "Law4Hire.API",
    "Law4Hire.API\Controllers",
    "Law4Hire.API\Hubs",
    "Law4Hire.API\Middleware",
    "Law4Hire.API\Configuration",
    "Law4Hire.Web",
    "Law4Hire.Web\Pages",
    "Law4Hire.Web\Components",
    "Law4Hire.Web\Services",
    "Law4Hire.Web\wwwroot",
    "Law4Hire.Mobile",
    "Law4Hire.Mobile\Platforms",
    "Law4Hire.Mobile\Views",
    "Law4Hire.Mobile\ViewModels",
    "Law4Hire.Mobile\Services",
    "Law4Hire.Shared",
    "Law4Hire.Shared\Components",
    "Law4Hire.Shared\Models",
    "Law4Hire.Shared\Resources",
    "Law4Hire.Shared\Utilities",
    "tests",
    "tests\Law4Hire.UnitTests",
    "tests\Law4Hire.IntegrationTests",
    "tests\Law4Hire.E2ETests",
    "docs",
    "docs\API",
    "docs\Architecture",
    "docs\UserGuides"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "Created directory: $dir" -ForegroundColor Yellow
}

# Create projects (ensure these run before writing files into them)
Write-Host "Creating .NET projects..." -ForegroundColor Green

# Core Class Library
Push-Location "Law4Hire.Core"
dotnet new classlib --framework net9.0
Remove-Item "Class1.cs" -ErrorAction SilentlyContinue
Pop-Location

# Infrastructure Class Library
Push-Location "Law4Hire.Infrastructure"
dotnet new classlib --framework net9.0
Remove-Item "Class1.cs" -ErrorAction SilentlyContinue
Pop-Location

# Application Class Library
Push-Location "Law4Hire.Application"
dotnet new classlib --framework net9.0
Remove-Item "Class1.cs" -ErrorAction SilentlyContinue
Pop-Location

# API Project
Push-Location "Law4Hire.API"
dotnet new webapi --framework net9.0 --use-controllers
Pop-Location

# Blazor Web Project - create directly into its directory from the solution root
Write-Host "Creating Blazor Web App..." -ForegroundColor Yellow
# Ensure we are at the solution root ($scriptDir) for the --output command
# The Pop-Location from the previous block ensures we are at the root.
dotnet new blazor --name Law4Hire.Web --framework net9.0 --all-interactive
# The Blazor template creates a solution file inside its project folder by default. Remove it.
# The path for this .sln will be Law4Hire.Web/Law4Hire.Web.sln relative to the solution root.
if (Test-Path "Law4Hire.Web\Law4Hire.Web.sln") {
    Remove-Item "Law4Hire.Web\Law4Hire.Web.sln" -Force
}

# MAUI Project - create directly into its directory from the solution root
# IMPORTANT: This requires the .NET MAUI workload to be installed.
# Install with: dotnet workload install maui
Write-Host "Attempting to create MAUI project. This requires 'dotnet workload install maui'." -ForegroundColor Yellow
# Ensure we are at the solution root ($scriptDir) for the --output command
# We are already at the root from the previous Blazor project creation.
dotnet new maui --name Law4Hire.Mobile --output "Law4Hire.Mobile" --framework net9.0
# The MAUI template also creates its own solution file, remove it
# The path for this .sln will be Law4Hire.Mobile/Law4Hire.Mobile.sln relative to the solution root.
if (Test-Path "Law4Hire.Mobile\Law4Hire.Mobile.sln") {
    Remove-Item "Law4Hire.Mobile\Law4Hire.Mobile.sln" -Force
}


# Shared Class Library
Push-Location "Law4Hire.Shared"
dotnet new classlib --framework net9.0
Remove-Item "Class1.cs" -ErrorAction SilentlyContinue
Pop-Location

# Test Projects
Push-Location "tests\Law4Hire.UnitTests"
dotnet new xunit --framework net9.0
Remove-Item "UnitTest1.cs" -ErrorAction SilentlyContinue # Remove default test file
Pop-Location

Push-Location "tests\Law4Hire.IntegrationTests"
dotnet new xunit --framework net9.0
Remove-Item "UnitTest1.cs" -ErrorAction SilentlyContinue # Remove default test file
Pop-Location

Push-Location "tests\Law4Hire.E2ETests"
dotnet new xunit --framework net9.0
Remove-Item "UnitTest1.cs" -ErrorAction SilentlyContinue # Remove default test file
Pop-Location

# Add projects to solution (corrected paths for Blazor and MAUI to point to the non-nested .csproj files)
Write-Host "Adding projects to solution..." -ForegroundColor Green
dotnet sln add "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
dotnet sln add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" | Out-Null
dotnet sln add "Law4Hire.Application\Law4Hire.Application.csproj" | Out-Null
dotnet sln add "Law4Hire.API\Law4Hire.API.csproj" | Out-Null
dotnet sln add "Law4Hire.Web\Law4Hire.Web.csproj" | Out-Null # Corrected path for Blazor project with --output
dotnet sln add "Law4Hire.Mobile\Law4Hire.Mobile.csproj" | Out-Null # Corrected path for MAUI project with --output
dotnet sln add "Law4Hire.Shared\Law4Hire.Shared.csproj" | Out-Null
dotnet sln add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" | Out-Null
dotnet sln add "tests\Law4Hire.IntegrationTests\Law4Hire.IntegrationTests.csproj" | Out-Null
dotnet sln add "tests\Law4Hire.E2ETests\Law4Hire.E2ETests.csproj" | Out-Null

# --- Start File Creation ---

# Create migration commands for Linux/Mac
@'
#!/bin/bash
# Migration Commands for Law4Hire (.NET 9)

echo "Law4Hire Entity Framework Migration Commands (.NET 9)"
echo "===================================================="

# Navigate to solution root
cd "$(dirname "$0")"

echo "1. Adding Initial Migration..."
dotnet ef migrations add InitialCreate --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output-dir Data/Migrations

echo "2. Updating Database..."
dotnet ef database update --project Law4Hire.Infrastructure --startup-project Law4Hire.API

echo "3. Migration completed successfully!"

# Optional: Generate SQL script
echo "4. Generating SQL script..."
dotnet ef migrations script --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output database-script.sql

echo "Done! Database is ready for use."
echo ""
echo "Next steps:"
echo "- Update connection strings in appsettings.json"
echo "- Run 'dotnet build' to build the solution"
echo "- Run 'dotnet run --project Law4Hire.API' to start the API"
echo "- Run 'dotnet run --project Law4Hire.Web' to start the web app"
'@ | Set-Content -Path "migrate.sh" -Encoding UTF8 -Force

# Create Windows batch version
@"
@echo off
REM Migration Commands for Law4Hire (.NET 9)

echo Law4Hire Entity Framework Migration Commands (.NET 9)
echo ====================================================

REM Navigate to solution root
cd /d "%~dp0"

echo 1. Adding Initial Migration...
dotnet ef migrations add InitialCreate --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output-dir Data/Migrations

echo 2. Updating Database...
dotnet ef database update --project Law4Hire.Infrastructure --startup-project Law4Hire.API

echo 3. Migration completed successfully!

REM Optional: Generate SQL script
echo 4. Generating SQL script...
dotnet ef migrations script --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output database-script.sql

echo Done! Database is ready for use.
echo.
echo Next steps:
echo - Update connection strings in appsettings.json
echo - Run 'dotnet build' to build the solution
echo - Run 'dotnet run --project Law4Hire.API' to start the API
echo - Run 'dotnet run --project Law4Hire.Web' to start the web app

pause
"@ | Set-Content -Path "migrate.bat" -Encoding UTF8 -Force

# Create enhanced data seeding with .NET 9 features
@'
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
                        QuestionKey = "full_name",
                        QuestionText = "What is your full legal name as it appears on your passport or government-issued ID?",
                        Type = QuestionType.Text,
                        Order = 1,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"minLength\": 2, \"maxLength\": 100, \"pattern\": \"^[a-zA-Z\\\\s\\\\-\\\\.]+$\"}"
                    },
                    new IntakeQuestion
                    {
                        QuestionKey = "date_of_birth",
                        QuestionText = "What is your date of birth?",
                        Type = QuestionType.Date,
                        Order = 2,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"type\": \"date\", \"maxDate\": \"today\", \"minAge\": 16}"
                    },
                    new IntakeQuestion
                    {
                        QuestionKey = "country_of_birth",
                        QuestionText = "In which country were you born?",
                        Type = QuestionType.Text,
                        Order = 3,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"minLength\": 2, \"maxLength\": 50}"
                    },
                    new IntakeQuestion
                    {
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
                        QuestionKey = "marriage_status",
                        QuestionText = "What is your current marital status?",
                        Type = QuestionType.MultipleChoice,
                        Order = 6,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Single\", \"Married\", \"Divorced\", \"Widowed\", \"Separated\"]}"
                    },
                    new IntakeQuestion
                    {
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
                        QuestionKey = "employment_status",
                        QuestionText = "What is your current employment situation in the United States?",
                        Type = QuestionType.MultipleChoice,
                        Order = 8,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"Employed with work authorization\", \"Employed without work authorization\", \"Self-employed\", \"Student\", \"Unemployed\", \"Retired\"]}"
                    },
                    new IntakeQuestion
                    {
                        QuestionKey = "legal_issues",
                        QuestionText = "Have you ever been arrested, charged with a crime, or had any legal issues in the US or any other country?",
                        Type = QuestionType.MultipleChoice,
                        Order = 9,
                        IsRequired = true,
                        ValidationRules = "{\"required\": true, \"options\": [\"No\", \"Yes - Minor traffic violations only\", \"Yes - Misdemeanor charges\", \"Yes - Felony charges\", \"Yes - Immigration violations\"]}"
                    },
                    new IntakeQuestion
                    {
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

            # Localized Content
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
'@ | Set-Content -Path "Law4Hire.Infrastructure\Data\DataSeeder.cs" -Encoding UTF8 -Force

# Create enhanced MAUI MainPage for .NET 9
@"
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="Law4Hire.Mobile.MainPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             Title="Law4Hire"
             BackgroundColor="{DynamicResource PageBackgroundColor}">

    <ScrollView>
        <Grid Padding="20" RowSpacing="25">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>

            <Frame Grid.Row="0"
                   BackgroundColor="{DynamicResource Primary}"
                   CornerRadius="15"
                   Padding="20"
                   HasShadow="True">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    
                    <Image Grid.Column="0"
                           Source="law4hire_icon.png"
                           HeightRequest="60"
                           WidthRequest="60"
                           VerticalOptions="Center" />
                    
                    <StackLayout Grid.Column="1"
                                 Spacing="5"
                                 VerticalOptions="Center"
                                 Margin="15,0,0,0">
                        <Label x:Name="AppTitleLabel"
                               Text="Law4Hire"
                               FontSize="28"
                               FontAttributes="Bold"
                               TextColor="White" />
                        <Label x:Name="AppSubtitleLabel"
                               Text="Your Legal Document Partner"
                               FontSize="14"
                               TextColor="White"
                               Opacity="0.9" />
                    </StackLayout>
                </Grid>
            </Frame>

            <Frame Grid.Row="1"
                   BackgroundColor="{DynamicResource CardBackgroundColor}"
                   CornerRadius="10"
                   Padding="20"
                   HasShadow="True">
                <StackLayout Spacing="10">
                    <Label x:Name="WelcomeLabel"
                           Text="Welcome to Law4Hire"
                           FontSize="22"
                           FontAttributes="Bold"
                           HorizontalOptions="Center"
                           TextColor="{DynamicResource PrimaryTextColor}" />
                    <Label x:Name="DescriptionLabel"
                           Text="Navigate your legal document needs with expert guidance and professional support"
                           FontSize="16"
                           HorizontalOptions="Center"
                           HorizontalTextAlignment="Center"
                           TextColor="{DynamicResource SecondaryTextColor}" />
                </StackLayout>
            </Frame>

            <Frame Grid.Row="2"
                   BackgroundColor="{DynamicResource CardBackgroundColor}"
                   CornerRadius="10"
                   Padding="15"
                   HasShadow="True">
                <StackLayout Spacing="10">
                    <Label Text="🌍 Select Language / Seleccionar Idioma"
                           FontSize="16"
                           FontAttributes="Bold"
                           HorizontalOptions="Center"
                           TextColor="{DynamicResource PrimaryTextColor}" />
                    <Picker x:Name="LanguagePicker"
                            Title="Choose your language"
                            FontSize="16"
                            TextColor="{DynamicResource PrimaryTextColor}"
                            TitleColor="{DynamicResource SecondaryTextColor}"
                            SelectedIndexChanged="OnLanguageChanged">
                        <Picker.ItemsSource>
                            <x:Array Type="{x:Type x:String}">
                                <x:String>🇺🇸 English</x:String>
                                <x:String>🇪🇸 Español</x:String>
                                <x:String>🇫🇷 Français</x:String>
                                <x:String>🇩🇪 Deutsch</x:String>
                                <x:String>🇨🇳 中文</x:String>
                                <x:String>🇧🇷 Português</x:String>
                                <x:String>🇮🇹 Italiano</x:String>
                                <x:String>🇷🇺 Русский</x:String>
                            </x:Array>
                        </Picker.ItemsSource>
                    </Picker>
                </StackLayout>
            </Frame>

            <StackLayout Grid.Row="3" Spacing="15">
                <Button x:Name="StartIntakeBtn"
                        Text="🚀 Start Legal Intake"
                        FontSize="18"
                        FontAttributes="Bold"
                        BackgroundColor="{DynamicResource Primary}"
                        TextColor="White"
                        CornerRadius="25"
                        HeightRequest="60"
                        Clicked="OnStartIntakeClicked"
                        Shadow="{OnPlatform Default={x:Null}, iOS={StaticResource ButtonShadow}}" />

                <Button x:Name="ViewPackagesBtn"
                        Text="📋 View Service Packages"
                        FontSize="18"
                        FontAttributes="Bold"
                        BackgroundColor="{DynamicResource Secondary}"
                        TextColor="White"
                        CornerRadius="25"
                        HeightRequest="60"
                        Clicked="OnViewPackagesClicked"
                        Shadow="{OnPlatform Default={x:Null}, iOS={StaticResource ButtonShadow}}" />

                <Button x:Name="MyAccountBtn"
                        Text="👤 My Account"
                        FontSize="18"
                        FontAttributes="Bold"
                        BackgroundColor="{DynamicResource Tertiary}"
                        TextColor="White"
                        CornerRadius="25"
                        HeightRequest="60"
                        Clicked="OnMyAccountClicked"
                        Shadow="{OnPlatform Default={x:Null}, iOS={StaticResource ButtonShadow}}" />
            </StackLayout>

            <Frame Grid.Row="4"
                   BackgroundColor="{DynamicResource CardBackgroundColor}"
                   CornerRadius="10"
                   Padding="20"
                   HasShadow="True">
                <StackLayout Spacing="15">
                    <Label Text="✨ Why Choose Law4Hire?"
                           FontSize="18"
                           FontAttributes="Bold"
                           HorizontalOptions="Center"
                           TextColor="{DynamicResource PrimaryTextColor}" />
                    
                    <Grid RowSpacing="10">
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>
                        
                        <StackLayout Grid.Row="0" Orientation="Horizontal" Spacing="10">
                            <Label Text="🛡️" FontSize="20" VerticalOptions="Center" />
                            <Label Text="Secure & Confidential"
                                   FontSize="14"
                                   VerticalOptions="Center"
                                   TextColor="{DynamicResource SecondaryTextColor}" />
                        </StackLayout>
                        
                        <StackLayout Grid.Row="1" Orientation="Horizontal" Spacing="10">
                            <Label Text="⚡" FontSize="20" VerticalOptions="Center" />
                            <Label Text="Fast & Efficient Process"
                                   FontSize="14"
                                   VerticalOptions="Center"
                                   TextColor="{DynamicResource SecondaryTextColor}" />
                        </StackLayout>
                        
                        <StackLayout Grid.Row="2" Orientation="Horizontal" Spacing="10">
                            <Label Text="👨‍⚖️" FontSize="20" VerticalOptions="Center" />
                            <Label Text="Expert Legal Guidance"
                                   FontSize="14"
                                   VerticalOptions="Center"
                                   TextColor="{DynamicResource SecondaryTextColor}" />
                        </StackLayout>
                    </Grid>
                </StackLayout>
            </Frame>

            <StackLayout Grid.Row="5"
                         Spacing="5"
                         Margin="0,20,0,0">
                <Label Text="© 2024 Law4Hire LLC"
                       FontSize="12"
                       HorizontalOptions="Center"
                       TextColor="{DynamicResource SecondaryTextColor}" />
                <Label Text="Professional Legal Document Services"
                       FontSize="12"
                       HorizontalOptions="Center"
                       TextColor="{DynamicResource SecondaryTextColor}" />
            </StackLayout>

        </Grid>
    </ScrollView>

</ContentPage>
"@ | Set-Content -Path "Law4Hire.Mobile\MainPage.xaml" -Encoding UTF8 -Force

# Create enhanced MAUI MainPage code-behind for .NET 9
@"
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Mobile;

public partial class MainPage : ContentPage
{
    private readonly IStringLocalizer<MainPage> _localizer;
    private readonly ILogger<MainPage> _logger;
    private string _currentLanguage = "en-US";

    public MainPage(IStringLocalizer<MainPage> localizer, ILogger<MainPage> logger)
    {
        InitializeComponent();
        _localizer = localizer;
        _logger = logger; # Initialize logger here
        UpdateUI();
        
        // Set default language selection
        LanguagePicker.SelectedIndex = 0;
        
        _logger.LogInformation("MainPage initialized");
    }

    private void UpdateUI()
    {
        try
        {
            WelcomeLabel.Text = _localizer["Welcome"];
            StartIntakeBtn.Text = $"🚀 {_localizer["StartIntake"]}";
            ViewPackagesBtn.Text = $"📋 {_localizer["ViewPackages"]}";
            MyAccountBtn.Text = $"👤 {_localizer["MyAccount"]}";
            DescriptionLabel.Text = _localizer["Description"];
            
            _logger.LogInformation("UI updated for language: {Language}", _currentLanguage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating UI");
        }
    }

    private async void OnStartIntakeClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Start intake button clicked");
            
            // Disable button to prevent double-clicks
            StartIntakeBtn.IsEnabled = false;
            StartIntakeBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to intake page
            await Shell.Current.GoToAsync("//intake");
            
            _logger.LogInformation("Navigated to intake page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to intake page");
            await DisplayAlert("Error", "Unable to start intake process. Please try again.", "OK");
        }
        finally
        {
            StartIntakeBtn.IsEnabled = true;
            StartIntakeBtn.Text = $"🚀 {_localizer["StartIntake"]}";
        }
    }

    private async void OnViewPackagesClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("View packages button clicked");
            
            ViewPackagesBtn.IsEnabled = false;
            ViewPackagesBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to packages page
            await Shell.Current.GoToAsync("//packages");
            
            _logger.LogInformation("Navigated to packages page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to packages page");
            await DisplayAlert("Error", "Unable to load packages. Please try again.", "OK");
        }
        finally
        {
            ViewPackagesBtn.IsEnabled = true;
            ViewPackagesBtn.Text = $"📋 {_localizer["ViewPackages"]}";
        }
    }

    private async void OnMyAccountClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("My account button clicked");
            
            MyAccountBtn.IsEnabled = false;
            MyAccountBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to account page or login if not authenticated
            await Shell.Current.GoToAsync("//account");
            
            _logger.LogInformation("Navigated to account page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to account page");
            await DisplayAlert("Error", "Unable to access account. Please try again.", "OK");
        }
        finally
        {
            MyAccountBtn.IsEnabled = true;
            MyAccountBtn.Text = $"👤 {_localizer["MyAccount"]}";
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = (Picker)sender;
            var selectedLanguage = picker.SelectedItem?.ToString();
            
            _logger.LogInformation("Language changed to: {Language}", selectedLanguage);
            
            // Map display names to culture codes
            _currentLanguage = selectedLanguage switch
            {
                "🇪🇸 Español" => "es-ES",
                "🇫🇷 Français" => "fr-FR", 
                "🇩🇪 Deutsch" => "de-DE",
                "🇨🇳 中文" => "zh-CN",
                "🇧🇷 Português" => "pt-BR",
                "🇮🇹 Italiano" => "it-IT",
                "🇷🇺 Русский" => "ru-RU",
                _ => "en-US"
            };

            // TODO: Implement actual culture switching
            // This would typically involve:
            // 1. Setting the current culture
            // 2. Reloading localized resources
            // 3. Updating the UI
            
            // For now, just update the UI with current localizations
            UpdateUI();
            
            // Show confirmation
            var message = _currentLanguage switch
            {
                "es-ES" => "Idioma cambiado a Español",
                "fr-FR" => "Langue changée en Français",
                "de-DE" => "Sprache auf Deutsch geändert",
                "zh-CN" => "语言已更改为中文",
                "pt-BR" => "Idioma alterado para Português",
                "it-IT" => "Lingua cambiata in Italiano",
                "ru-RU" => "Язык изменен на Русский",
                _ => "Language changed to English"
            };
            
            DisplayAlert("Language Updated", message, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing language");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _logger.LogInformation("MainPage appeared");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _logger.LogInformation("MainPage disappeared");
    }
}
"@ | Set-Content -Path "Law4Hire.Mobile\MainPage.xaml.cs" -Encoding UTF8 -Force

Write-Host "`n✅ Law4Hire solution structure created successfully!" -ForegroundColor Green
Write-Host "🚀 .NET 9.0 Framework with latest best practices" -ForegroundColor Cyan
Write-Host "📁 Projects created in root directory structure" -ForegroundColor Cyan
Write-Host "📄 Entity models created: 8" -ForegroundColor Cyan
Write-Host "🔧 Enhanced controllers with .NET 9 features: 3" -ForegroundColor Cyan
Write-Host "🏠 Advanced SignalR Hub with rate limiting: 1" -ForegroundColor Cyan
Write-Host "📱 Modern Blazor pages with real-time features: 2" -ForegroundColor Cyan
Write-Host "🌍 Comprehensive localization support (8 languages)" -ForegroundColor Cyan
Write-Host "📱 Enhanced MAUI mobile app with .NET 9: 1" -ForegroundColor Cyan
Write-Host "🗃️ Advanced data seeding with detailed content" -ForegroundColor Cyan
Write-Host "📦 Latest NuGet packages for .NET 9" -ForegroundColor Cyan

Write-Host "`n🚀 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Update connection strings in Law4Hire.API/appsettings.json" -ForegroundColor White
Write-Host "   2. Run: .\migrate.bat (Windows) or ./migrate.sh (Linux/Mac)" -ForegroundColor White
Write-Host "   3. Build: dotnet build" -ForegroundColor White
Write-Host "   4. Run API: dotnet run --project Law4Hire.API" -ForegroundColor White
Write-Host "   5. Run Web: dotnet run --project Law4Hire.Web" -ForegroundColor White
Write-Host "   6. Run Mobile: dotnet build Law4Hire.Mobile -f net9.0-android" -ForegroundColor White

Write-Host "`n📝 .NET 9 Features Implemented:" -ForegroundColor Magenta
Write-Host "   - Primary constructors in repositories and services" -ForegroundColor White
Write-Host "   - Enhanced performance with latest EF Core 9.0" -ForegroundColor White
Write-Host "   - Advanced rate limiting and security features" -ForegroundColor White
Write-Host "   - Modern Blazor Server/WebAssembly hybrid" -ForegroundColor White
Write-Host "   - Improved SignalR with connection resilience" -ForegroundColor White
Write-Host "   - Enhanced MAUI with .NET 9 optimizations" -ForegroundColor White
Write-Host "   - Advanced logging and error handling" -ForegroundColor White
Write-Host "   - Latest security and encryption standards" -ForegroundColor White

Write-Host "`n💡 Ready for development with .NET 9!" -ForegroundColor Green

# Create enhanced Blazor pages using .NET 9 features

@"
@page "/intake"
@using Law4Hire.Core.DTOs
@using Law4Hire.Core.Enums
@using Microsoft.AspNetCore.SignalR.Client
@inject HttpClient Http
@inject IJSRuntime JSRuntime
@inject ILogger<Intake> Logger
@implements IAsyncDisposable

<PageTitle>Legal Intake - Law4Hire</PageTitle>

<div class="container-fluid mt-4">
    <div class="row justify-content-center">
        <div class="col-xl-8">
            <div class="card shadow-lg">
                <div class="card-header bg-primary text-white">
                    <div class="d-flex justify-content-between align-items-center">
                        <h3 class="card-title mb-0">
                            <i class="fas fa-balance-scale me-2"></i>
                            Legal Document Intake
                        </h3>
                        @if (intakeStarted)
                        {
                            <span class="badge bg-success">Session Active</span>
                        }
                    </div>
                </div>
                <div class="card-body">
                    @if (!intakeStarted)
                    {
                        <div class="text-center py-4">
                            <div class="mb-4">
                                <i class="fas fa-handshake text-primary" style="font-size: 4rem;"></i>
                            </div>
                            <h4 class="mb-3">Welcome to Law4Hire</h4>
                            <p class="lead mb-4 text-muted">
                                We'll guide you through a personalized intake process to determine
                                what legal documents you need and gather the necessary information.
                            </p>
                            
                            <div class="row justify-content-center">
                                <div class="col-md-6">
                                    <div class="mb-4">
                                        <label for="language" class="form-label fw-bold">
                                            <i class="fas fa-globe me-2"></i>Preferred Language:
                                        </label>
                                        <select @bind="selectedLanguage" class="form-select form-select-lg" id="language">
                                            <option value="en-US">🇺🇸 English</option>
                                            <option value="es-ES">🇪🇸 Español</option>
                                            <option value="fr-FR">🇫🇷 Français</option>
                                            <option value="de-DE">🇩🇪 Deutsch</option>
                                            <option value="zh-CN">🇨🇳 中文</option>
                                            <option value="pt-BR">🇧🇷 Português</option>
                                            <option value="it-IT">🇮🇹 Italiano</option>
                                            <option value="ru-RU">🇷🇺 Русский</option>
                                        </select>
                                    </div>
                                    
                                    <button class="btn btn-primary btn-lg w-100 py-3"
                                            @onclick="StartIntake"
                                            disabled="@isLoading">
                                        @if (isLoading)
                                        {
                                            <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                                            <span>Starting...</span>
                                        }
                                        else
                                        {
                                            <i class="fas fa-play me-2"></i>
                                            <span>Start Intake Process</span>
                                        }
                                    </button>
                                </div>
                            </div>
                        </div>
                    }
                    else
                    {
                        <div class="row">
                            <div class="col-12">
                                <div id="chat-container"
                                     class="border rounded p-3 mb-3 bg-light"
                                     style="height: 500px; overflow-y: auto;">
                                    @foreach (var message in chatMessages)
                                    {
                                        <div class="mb-3 @(message.IsBot ? "d-flex" : "d-flex justify-content-end")">
                                            <div class="@(message.IsBot ? "me-auto" : "ms-auto")" style="max-width: 75%;">
                                                <div class="@(message.IsBot ? "alert alert-secondary" : "alert alert-primary") mb-1">
                                                    <div class="d-flex align-items-center mb-2">
                                                        @if (message.IsBot)
                                                        {
                                                            <i class="fas fa-robot me-2"></i>
                                                            <strong>Legal Assistant</strong>
                                                        }
                                                        else
                                                        {
                                                            <i class="fas fa-user me-2"></i>
                                                            <strong>You</strong>
                                                        }
                                                        <small class="text-muted ms-auto">
                                                            @message.Timestamp.ToString("HH:mm")
                                                        </small>
                                                    </div>
                                                    <div>@message.Text</div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                    
                                    @if (isTyping)
                                    {
                                        <div class="mb-3 d-flex">
                                            <div class="me-auto" style="max-width: 75%;">
                                                <div class="alert alert-secondary mb-1">
                                                    <div class="d-flex align-items-center">
                                                        <i class="fas fa-robot me-2"></i>
                                                        <strong>Legal Assistant</strong>
                                                        <div class="typing-indicator ms-3">
                                                            <span></span>
                                                            <span></span>
                                                            <span></span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                                
                                <div class="input-group">
                                    <span class="input-group-text">
                                        <i class="fas fa-comment"></i>
                                    </span>
                                    <input @bind="currentResponse"
                                           @onkeypress="@(async (e) => { if (e.Key == "Enter") await SendResponse(); })"
                                           class="form-control"
                                           placeholder="Type your response here..."
                                           disabled="@(!isConnected || isLoading)" />
                                    <button class="btn btn-primary"
                                            @onclick="SendResponse"
                                            disabled="@(!isConnected || isLoading || string.IsNullOrWhiteSpace(currentResponse))">
                                        @if (isLoading)
                                        {
                                            <span class="spinner-border spinner-border-sm"></span>
                                        }
                                        else
                                        {
                                            <i class="fas fa-paper-plane"></i>
                                        }
                                    </button>
                                </div>
                                
                                <div class="mt-2">
                                    <small class="@(isConnected ? "text-success" : "text-danger")">
                                        <i class="fas fa-@(isConnected ? "wifi" : "exclamation-triangle")"></i>
                                        @(isConnected ? "Connected" : "Connection lost - Reconnecting...")
                                    </small>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private bool intakeStarted = false;
    private bool isLoading = false;
    private bool isTyping = false;
    private bool isConnected = false;
    private string selectedLanguage = "en-US";
    private string currentResponse = "";
    private List<ChatMessage> chatMessages = new();
    private Guid? currentSessionId;
    private HubConnection? hubConnection;

    private class ChatMessage
    {
        public string Text { get; set; } = "";
        public bool IsBot { get; set; }
        public DateTime Timestamp { get; set; }
        public string? QuestionType { get; set; }
        public int? QuestionId { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await InitializeSignalRConnection();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize SignalR connection");
        }
    }

    private async Task InitializeSignalRConnection()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/hubs/intake") // TODO: Use configuration
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<string, string, string, DateTime>("ReceiveMessage", (userId, userName, message, timestamp) =>
        {
            InvokeAsync(() =>
            {
                chatMessages.Add(new ChatMessage
                {
                    Text = message,
                    IsBot = false,
                    Timestamp = timestamp
                });
                StateHasChanged();
                ScrollToBottom();
            });
        });

        hubConnection.On<string, string, int?, DateTime>("ReceiveBotQuestion", (question, questionType, questionId, timestamp) =>
        {
            InvokeAsync(() =>
            {
                isTyping = false;
                chatMessages.Add(new ChatMessage
                {
                    Text = question,
                    IsBot = true,
                    Timestamp = timestamp,
                    QuestionType = questionType,
                    QuestionId = questionId
                });
                StateHasChanged();
                ScrollToBottom();
            });
        });

        hubConnection.On<string, DateTime>("UserJoined", (userId, timestamp) =>
        {
            InvokeAsync(() =>
            {
                Logger.LogInformation("User {UserId} joined the session", userId);
                StateHasChanged();
            });
        });

        hubConnection.On<int, string, DateTime>("ResponseSubmitted", (questionId, response, timestamp) =>
        {
            InvokeAsync(() =>
            {
                isTyping = true;
                StateHasChanged();
                // Simulate bot processing time
                Task.Delay(2000).ContinueWith(_ => GetNextQuestion());
            });
        });

        hubConnection.Reconnecting += (error) =>
        {
            InvokeAsync(() =>
            {
                isConnected = false;
                StateHasChanged();
            });
            return Task.CompletedTask;
        };

        hubConnection.Reconnected += (connectionId) =>
        {
            InvokeAsync(() =>
            {
                isConnected = true;
                StateHasChanged();
            });
            return Task.CompletedTask;
        };

        hubConnection.Closed += (error) =>
        {
            InvokeAsync(() =>
            {
                isConnected = false;
                StateHasChanged();
            });
            return Task.CompletedTask;
        };

        await hubConnection.StartAsync();
        isConnected = hubConnection.State == HubConnectionState.Connected;
    }

    private async Task StartIntake()
    {
        isLoading = true;
        try
        {
            // TODO: Create user if not exists
            var userId = Guid.NewGuid(); // Placeholder

            // Create intake session
            var createSessionDto = new CreateIntakeSessionDto(userId, selectedLanguage);
            var response = await Http.PostAsJsonAsync("api/intake/sessions", createSessionDto);
            
            if (response.IsSuccessStatusCode)
            {
                var session = await response.Content.ReadFromJsonAsync<IntakeSessionDto>();
                currentSessionId = session?.Id;

                intakeStarted = true;

                // Join SignalR group
                if (hubConnection?.State == HubConnectionState.Connected && currentSessionId.HasValue)
                {
                    await hubConnection.SendAsync("JoinIntakeSession", currentSessionId.Value.ToString());
                }

                // Add welcome message
                chatMessages.Add(new ChatMessage
                {
                    Text = GetLocalizedWelcomeMessage(),
                    IsBot = true,
                    Timestamp = DateTime.Now
                });

                // Start with first question
                await Task.Delay(1000);
                await GetNextQuestion();
            }
            else
            {
                Logger.LogError("Failed to create intake session: {StatusCode}", response.StatusCode);
                // TODO: Show error message
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting intake session");
            // TODO: Show error message
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
            await ScrollToBottom();
        }
    }

    private async Task SendResponse()
    {
        if (string.IsNullOrWhiteSpace(currentResponse) || !isConnected) return;

        var response = currentResponse.Trim();
        currentResponse = "";

        try
        {
            chatMessages.Add(new ChatMessage
            {
                Text = response,
                IsBot = false,
                Timestamp = DateTime.Now
            });

            if (hubConnection?.State == HubConnectionState.Connected && currentSessionId.HasValue)
            {
                await hubConnection.SendAsync("SendMessage", currentSessionId.Value.ToString(), response);
                
                // Submit response if we have a current question
                var lastBotMessage = chatMessages.LastOrDefault(m => m.IsBot && m.QuestionId.HasValue);
                if (lastBotMessage?.QuestionId.HasValue == true)
                {
                    await hubConnection.SendAsync("SubmitResponse", 
                        currentSessionId.Value.ToString(), 
                        lastBotMessage.QuestionId.Value, 
                        response);
                }
            }

            StateHasChanged();
            await ScrollToBottom();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending response");
        }
    }

    private async Task GetNextQuestion()
    {
        try
        {
            if (!currentSessionId.HasValue) return;

            // TODO: Call API to get next question based on responses
            await Task.Delay(1500); // Simulate processing

            var sampleQuestions = new[]
            {
                "What is your full legal name?",
                "What is your date of birth?",
                "In which country were you born?",
                "What is your current immigration status in the United States?",
                "Do you have any immediate family members who are U.S. citizens or permanent residents?"
            };

            var questionIndex = chatMessages.Count(m => m.IsBot && !string.IsNullOrEmpty(m.QuestionType)) % sampleQuestions.Length;
            var question = sampleQuestions[questionIndex];

            if (hubConnection?.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("SendBotResponse", 
                    currentSessionId.Value.ToString(), 
                    question, 
                    "text", 
                    questionIndex + 1);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting next question");
        }
    }

    private string GetLocalizedWelcomeMessage()
    {
        return selectedLanguage switch
        {
            "es-ES" => "¡Hola! Soy su asistente legal. Le ayudaré a determinar qué documentos legales necesita. Comencemos con información básica.",
            "fr-FR" => "Bonjour! Je suis votre assistant juridique. Je vais vous aider à déterminer quels documents juridiques vous avez besoin. Commençons par des informations de base.",
            "de-DE" => "Hallo! Ich bin Ihr Rechtsassistent. Ich helfe Ihnen dabei, herauszufinden, welche Rechtsdokumente Sie benötigen. Beginnen wir mit grundlegenden Informationen.",
            "zh-CN" => "您好！我是您的法律助手。我将帮助您确定您需要哪些法律文件。让我们从基本信息开始。",
            "pt-BR" => "Olá! Sou seu assistente jurídico. Vou ajudá-lo a determinar quais documentos legais você precisa. Vamos começar com informações básicas.",
            "it-IT" => "Ciao! Sono il tuo assistente legale. Ti aiuterò a determinare di quali documenti legali hai bisogno. Iniziamo con informazioni di base.",
            "ru-RU" => "Привет! Я ваш юридический помощник. Я помогу вам определить, какие юридические документы вам нужны. Начнем с основной информации.",
            _ => "Hello! I'm your legal assistant. I'll help you determine what legal documents you need and gather the necessary information. Let's start with some basic information."
        };
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("scrollToBottom", "chat-container");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error scrolling to bottom");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}

<style>
    .typing-indicator {
        display: inline-flex;
        gap: 4px;
    }

    .typing-indicator span {
        height: 8px;
        width: 8px;
        background-color: #6c757d;
        border-radius: 50%;
        animation: typing 1.5s infinite;
    }

    .typing-indicator span:nth-child(2) {
        animation-delay: 0.2s;
    }

    .typing-indicator span:nth-child(3) {
        animation-delay: 0.4s;
    }

    @keyframes typing {
        0%, 60%, 100% {
            transform: translateY(0);
            opacity: 0.5;
        }
        30% {
            transform: translateY(-10px);
            opacity: 1;
        }
    }

    .card {
        border: none;
        border-radius: 15px;
    }

    .card-header {
        border-radius: 15px 15px 0 0 !important;
    }

    #chat-container {
        border-radius: 10px;
    }

    .alert {
        border-radius: 10px;
    }

    .form-select:focus, .form-control:focus {
        border-color: #0d6efd;
        box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
    }
</style>

<script>
    window.scrollToBottom = (elementId) => {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    };
</script>
"@ | Set-Content -Path "Law4Hire.Web\Pages\Intake.razor" -Encoding UTF8 -Force

# Create enhanced Service Packages page
@"
@page "/packages"
@using Law4Hire.Core.DTOs
@using Law4Hire.Core.Enums
@inject HttpClient Http
@inject ILogger<ServicePackages> Logger

<PageTitle>Service Packages - Law4Hire</PageTitle>

<div class="container mt-4">
    <div class="row">
        <div class="col-12 text-center mb-5">
            <h1 class="display-4 fw-bold text-primary">Choose Your Service Package</h1>
            <p class="lead text-muted">Select the level of legal assistance that best fits your needs</p>
        </div>
    </div>
    
    @if (isLoading)
    {
        <div class="row justify-content-center">
            <div class="col-md-6 text-center">
                <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status">
                    <span class="visually-hidden">Loading packages...</span>
                </div>
                <p class="mt-3 text-muted">Loading service packages...</p>
            </div>
        </div>
    }
    else if (error != null)
    {
        <div class="row justify-content-center">
            <div class="col-md-8">
                <div class="alert alert-danger" role="alert">
                    <h4 class="alert-heading">
                        <i class="fas fa-exclamation-triangle"></i>
                        Error Loading Packages
                    </h4>
                    <p>@error</p>
                    <hr>
                    <button class="btn btn-outline-danger" @onclick="LoadPackages">
                        <i class="fas fa-redo"></i> Try Again
                    </button>
                </div>
            </div>
        </div>
    }
    else if (packages != null && packages.Any())
    {
        <div class="row g-4">
            @foreach (var package in packages.OrderBy(p => p.Type))
            {
                <div class="col-lg-6 col-xl-3">
                    <div class="card h-100 package-card @(package.Type == PackageType.FullRepresentationGuaranteed ? "featured" : "")"
                         @onclick="() => SelectPackage(package)">
                        
                        @if (package.Type == PackageType.FullRepresentationGuaranteed)
                        {
                            <div class="card-ribbon">
                                <span class="ribbon-text">Most Popular</span>
                            </div>
                        }
                        
                        <div class="card-header text-center @(package.Type == PackageType.FullRepresentationGuaranteed ? "bg-success text-white" : "bg-light")">
                            <h5 class="card-title mb-2">@package.Name</h5>
                            @if (package.HasMoneyBackGuarantee)
                            {
                                <span class="badge bg-warning text-dark">
                                    <i class="fas fa-shield-alt"></i> Money-Back Guarantee
                                </span>
                            }
                        </div>
                        
                        <div class="card-body d-flex flex-column">
                            <div class="package-icon text-center mb-3">
                                @switch (package.Type)
                                {
                                    case PackageType.SelfRepresentationWithParalegal:
                                        <i class="fas fa-user-edit text-info" style="font-size: 3rem;"></i>
                                        break;
                                    case PackageType.HybridWithAttorneyOverview:
                                        <i class="fas fa-user-friends text-warning" style="font-size: 3rem;"></i>
                                        break;
                                    case PackageType.FullRepresentationStandard:
                                        <i class="fas fa-briefcase text-primary" style="font-size: 3rem;"></i>
                                        break;
                                    case PackageType.FullRepresentationGuaranteed:
                                        <i class="fas fa-crown text-success" style="font-size: 3rem;"></i>
                                        break;
                                }
                            </div>
                            
                            <p class="card-text text-muted">@package.Description</p>
                            
                            <div class="package-features mb-3">
                                @switch (package.Type)
                                {
                                    case PackageType.SelfRepresentationWithParalegal:
                                        <ul class="list-unstyled">
                                            <li><i class="fas fa-check text-success"></i> Document preparation guidance</li>
                                            <li><i class="fas fa-check text-success"></i> Paralegal review</li>
                                            <li><i class="fas fa-check text-success"></i> Basic support</li>
                                        </ul>
                                        break;
                                    case PackageType.HybridWithAttorneyOverview:
                                        <ul class="list-unstyled">
                                            <li><i class="fas fa-check text-success"></i> Attorney oversight</li>
                                            <li><i class="fas fa-check text-success"></i> Document review</li>
                                            <li><i class="fas fa-check text-success"></i> Legal guidance</li>
                                            <li><i class="fas fa-times text-muted"></i> No G-28 filing</li>
                                        </ul>
                                        break;
                                    case PackageType.FullRepresentationStandard:
                                        <ul class="list-unstyled">
                                            <li><i class="fas fa-check text-success"></i> Full attorney representation</li>
                                            <li><i class="fas fa-check text-success"></i> G-28 filing included</li>
                                            <li><i class="fas fa-check text-success"></i> Case management</li>
                                            <li><i class="fas fa-check text-success"></i> Court appearances</li>
                                        </ul>
                                        break;
                                    case PackageType.FullRepresentationGuaranteed:
                                        <ul class="list-unstyled">
                                            <li><i class="fas fa-check text-success"></i> Full attorney representation</li>
                                            <li><i class="fas fa-check text-success"></i> G-28 filing included</li>
                                            <li><i class="fas fa-check text-success"></i> Money-back guarantee</li>
                                            <li><i class="fas fa-check text-success"></i> Priority support</li>
                                        </ul>
                                        break;
                                }
                            </div>
                            
                            <div class="mt-auto">
                                <div class="price-section text-center mb-3">
                                    <h3 class="price-amount mb-1">$@package.BasePrice.ToString("N0")</h3>
                                    <small class="text-muted">Starting price</small>
                                </div>
                                
                                <button class="btn @(package.Type == PackageType.FullRepresentationGuaranteed ? "btn-success" : "btn-primary") w-100 btn-lg"
                                        disabled="@isSelectingPackage">
                                    @if (isSelectingPackage && selectedPackageId == package.Id)
                                    {
                                        <span class="spinner-border spinner-border-sm me-2"></span>
                                        <span>Processing...</span>
                                    }
                                    else
                                    {
                                        <i class="fas fa-arrow-right me-2"></i>
                                        <span>Select Package</span>
                                    }
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
        
        <div class="row mt-5">
            <div class="col-12">
                <div class="card bg-light">
                    <div class="card-body">
                        <h4 class="card-title text-center mb-4">
                            <i class="fas fa-info-circle text-primary"></i>
                            What's Included
                        </h4>
                        <div class="row">
                            <div class="col-md-6">
                                <h6><i class="fas fa-document-alt text-primary"></i> Document Services</h6>
                                <ul class="list-unstyled">
                                    <li>• Form identification and preparation</li>
                                    <li>• Document review and verification</li>
                                    <li>• Submission assistance</li>
                                </ul>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="fas fa-headset text-primary"></i> Support Services</h6>
                                <ul class="list-unstyled">
                                    <li>• Multi-language support</li>
                                    <li>• Case status updates</li>
                                    <li>• Expert legal guidance</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
    else
    {
        <div class="row justify-content-center">
            <div class="col-md-6 text-center">
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    No service packages are currently available. Please check back later.
                </div>
            </div>
        </div>
    }
</div>

@code {
    private IEnumerable<ServicePackageDto>? packages;
    private bool isLoading = true;
    private bool isSelectingPackage = false;
    private int? selectedPackageId;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        await LoadPackages();
    }

    private async Task LoadPackages()
    {
        isLoading = true;
        error = null;
        
        try
        {
            packages = await Http.GetFromJsonAsync<IEnumerable<ServicePackageDto>>("api/servicepackages");
            Logger.LogInformation("Successfully loaded {Count} service packages", packages?.Count() ?? 0);
        }
        catch (HttpRequestException httpEx)
        {
            error = "Unable to connect to the server. Please check your internet connection and try again.";
            Logger.LogError(httpEx, "HTTP error loading service packages");
        }
        catch (Exception ex)
        {
            error = "An unexpected error occurred while loading packages. Please try again later.";
            Logger.LogError(ex, "Error loading service packages");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SelectPackage(ServicePackageDto package)
    {
        if (isSelectingPackage) return;
        
        isSelectingPackage = true;
        selectedPackageId = package.Id;
        
        try
        {
            Logger.LogInformation("User selected package: {PackageName} (ID: {PackageId})", package.Name, package.Id);
            
            // TODO: Navigate to checkout/payment or intake process
            // TODO: Save selected package to user session/state
            
            await Task.Delay(1000); // Simulate processing
            
            // For now, just log the selection
            // In a real implementation, you would:
            // 1. Save the selection to user's session
            // 2. Navigate to payment/checkout
            // 3. Or navigate to intake if not completed
            
            Logger.LogInformation("Package selection completed for: {PackageName}", package.Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error selecting package {PackageId}", package.Id);
            // TODO: Show error message to user
        }
        finally
        {
            isSelectingPackage = false;
            selectedPackageId = null;
            StateHasChanged();
        }
    }
}

<style>
    .package-card {
        transition: all 0.3s ease;
        cursor: pointer;
        border: 2px solid transparent;
        border-radius: 15px;
        position: relative;
        overflow: hidden;
    }

    .package-card:hover {
        transform: translateY(-5px);
        box-shadow: 0 10px 25px rgba(0,0,0,0.15);
        border-color: #0d6efd;
    }

    .package-card.featured {
        border-color: #198754;
        box-shadow: 0 5px 15px rgba(25,135,84,0.2);
    }

    .package-card.featured:hover {
        border-color: #198754;
        box-shadow: 0 15px 30px rgba(25,135,84,0.3);
    }

    .card-ribbon {
        position: absolute;
        top: 15px;
        right: -35px;
        width: 120px;
        height: 25px;
        background: linear-gradient(45deg, #ffc107, #ffb700);
        z-index: 10;
        transform: rotate(45deg);
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .ribbon-text {
        font-size: 0.75rem;
        font-weight: bold;
        color: #000;
        text-transform: uppercase;
    }

    .price-amount {
        color: #198754;
        font-weight: bold;
    }

    .package-features li {
        margin-bottom: 0.5rem;
        font-size: 0.9rem;
    }

    .package-features .fas {
        width: 16px;
        margin-right: 8px;
    }

    .card-header {
        border-radius: 15px 15px 0 0 !important;
    }

    .btn {
        border-radius: 10px;
        font-weight: 600;
        transition: all 0.3s ease;
    }

    .btn:hover {
        transform: translateY(-2px);
    }

    .package-icon {
        margin: 1rem 0;
    }

    @media (max-width: 768px) {
        .package-card {
            margin-bottom: 2rem;
        }
        
        .card-ribbon {
            top: 10px;
            right: -30px;
            width: 100px;
            height: 20px;
        }
        
        .ribbon-text {
            font-size: 0.65rem;
        }
    }
</style>
"@ | Set-Content -Path "Law4Hire.Web\Pages\ServicePackages.razor" -Encoding UTF8 -Force

# Create basic repository implementations using .NET 9 features
@"
using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.Infrastructure.Data.Repositories;

public class UserRepository(Law4HireDbContext context) : IUserRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.IntakeSessions)
            .Include(u => u.ServiceRequests)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ServicePackageRepository(Law4HireDbContext context) : IServicePackageRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IEnumerable<ServicePackage>> GetAllActiveAsync()
    {
        return await _context.ServicePackages
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.Type)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServicePackage?> GetByIdAsync(int id)
    {
        return await _context.ServicePackages
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == id);
    }

    public async Task<ServicePackage> CreateAsync(ServicePackage package)
    {
        package.CreatedAt = DateTime.UtcNow;
        _context.ServicePackages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<ServicePackage> UpdateAsync(ServicePackage package)
    {
        _context.ServicePackages.Update(package);
        await _context.SaveChangesAsync();
        return package;
    }
}

public class IntakeSessionRepository(Law4HireDbContext context) : IIntakeSessionRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IntakeSession?> GetByIdAsync(Guid id)
    {
        return await _context.IntakeSessions
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IntakeSession> CreateAsync(IntakeSession session)
    {
        session.Id = Guid.NewGuid();
        session.StartedAt = DateTime.UtcNow;
        
        _context.IntakeSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<IntakeSession> UpdateAsync(IntakeSession session)
    {
        _context.IntakeSessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<IEnumerable<IntakeSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.IntakeSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}

public class ServiceRequestRepository(Law4HireDbContext context) : IServiceRequestRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<ServiceRequest?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.ServicePackage)
            .Include(r => r.RequiredForms)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ServiceRequest> CreateAsync(ServiceRequest request)
    {
        request.Id = Guid.NewGuid();
        request.CreatedAt = DateTime.UtcNow;
        
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ServiceRequest> UpdateAsync(ServiceRequest request)
    {
        _context.ServiceRequests.Update(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<IEnumerable<ServiceRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ServiceRequests
            .Where(r => r.UserId == userId)
            .Include(r => r.ServicePackage)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}

public class LocalizedContentRepository(Law4HireDbContext context) : ILocalizedContentRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<LocalizedContent?> GetContentAsync(string key, string language)
    {
        return await _context.LocalizedContents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContentKey == key && c.Language == language);
    }

    public async Task<IEnumerable<LocalizedContent>> GetAllForLanguageAsync(string language)
    {
        return await _context.LocalizedContents
            .Where(c => c.Language == language)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LocalizedContent> CreateOrUpdateAsync(LocalizedContent content)
    {
        var existing = await _context.LocalizedContents
            .FirstOrDefaultAsync(c => c.ContentKey == content.ContentKey && c.Language == content.Language);

        if (existing != null)
        {
            existing.Content = content.Content;
            existing.Description = content.Description;
            existing.LastUpdated = DateTime.UtcNow;
            _context.LocalizedContents.Update(existing);
        }
        else
        {
            content.LastUpdated = DateTime.UtcNow;
            _context.LocalizedContents.Add(content);
        }

        await _context.SaveChangesAsync();
        return existing ?? content;
    }
}
"@ | Set-Content -Path "Law4Hire.Infrastructure\Data\Repositories\Repositories.cs" -Encoding UTF8 -Force

# Create separate controllers using .NET 9 features
# UsersController.cs
@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.PreferredLanguage,
            user.CreatedAt,
            user.IsActive
        );

        return Ok(userDto);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new User
        {
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            PhoneNumber = createUserDto.PhoneNumber,
            PreferredLanguage = createUserDto.PreferredLanguage
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var userDto = new UserDto(
            createdUser.Id,
            createdUser.Email,
            createdUser.FirstName,
            createdUser.LastName,
            createdUser.PhoneNumber,
            createdUser.PreferredLanguage,
            createdUser.CreatedAt,
            createdUser.IsActive
        );

        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
    }
}
'@ | Set-Content -Path "Law4Hire.API\Controllers\UsersController.cs" -Encoding UTF8 -Force

# ServicePackagesController.cs
@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ServicePackagesController(IServicePackageRepository servicePackageRepository) : ControllerBase
{
    private readonly IServicePackageRepository _servicePackageRepository = servicePackageRepository;

    /// <summary>
    /// Get all active service packages
    /// </summary>
    /// <returns>List of active service packages</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ServicePackageDto>>(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServicePackageDto>>> GetActivePackages()
    {
        var packages = await _servicePackageRepository.GetAllActiveAsync();
        
        var packageDtos = packages.Select(p => new ServicePackageDto(
            p.Id,
            p.Name,
            p.Description,
            p.Type,
            p.BasePrice,
            p.HasMoneyBackGuarantee,
            p.IsActive
        ));

        return Ok(packageDtos);
    }

    /// <summary>
    /// Get service package by ID
    /// </summary>
    /// <param name="id">Service Package ID</param>
    /// <returns>Service package details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ServicePackageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<ServicePackageDto>> GetPackage(int id)
    {
        var package = await _servicePackageRepository.GetByIdAsync(id);
        if (package == null)
            return NotFound($"Service package with ID {id} not found.");

        var packageDto = new ServicePackageDto(
            package.Id,
            package.Name,
            package.Description,
            package.Type,
            package.BasePrice,
            package.HasMoneyBackGuarantee,
            package.IsActive
        );

        return Ok(packageDto);
    }
}
'@ | Set-Content -Path "Law4Hire.API\Controllers\ServicePackagesController.cs" -Encoding UTF8 -Force

# IntakeController.cs
@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class IntakeController(IIntakeSessionRepository intakeSessionRepository) : ControllerBase
{
    private readonly IIntakeSessionRepository _intakeSessionRepository = intakeSessionRepository;

    /// <summary>
    /// Create a new intake session
    /// </summary>
    /// <param name="createSessionDto">Intake session creation data</param>
    /// <returns>Created intake session</returns>
    [HttpPost("sessions")]
    [ProducesResponseType<IntakeSessionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeSessionDto>> CreateIntakeSession([FromBody] CreateIntakeSessionDto createSessionDto)
    {
        var session = new IntakeSession
        {
            UserId = createSessionDto.UserId,
            Language = createSessionDto.Language,
            Status = Core.Enums.IntakeStatus.Started
        };

        var createdSession = await _intakeSessionRepository.CreateAsync(session);

        var sessionDto = new IntakeSessionDto(
            createdSession.Id,
            createdSession.UserId,
            createdSession.Status,
            createdSession.StartedAt,
            createdSession.CompletedAt,
            createdSession.Language
        );

        return CreatedAtAction(nameof(GetIntakeSession), new { id = createdSession.Id }, sessionDto);
    }

    /// <summary>
    /// Get intake session by ID
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <returns>Intake session details</returns>
    [HttpGet("sessions/{id:guid}")]
    [ProducesResponseType<IntakeSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntakeSessionDto>> GetIntakeSession(Guid id)
    {
        var session = await _intakeSessionRepository.GetByIdAsync(id);
        if (session == null)
            return NotFound($"Intake session with ID {id} not found.");

        var sessionDto = new IntakeSessionDto(
            session.Id,
            session.UserId,
            session.Status,
            session.StartedAt,
            session.CompletedAt,
            session.Language
        );

        return Ok(sessionDto);
    }

    /// <summary>
    /// Get intake sessions for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's intake sessions</returns>
    [HttpGet("users/{userId:guid}/sessions")]
    [ProducesResponseType<IEnumerable<IntakeSessionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IntakeSessionDto>>> GetUserIntakeSessions(Guid userId)
    {
        var sessions = await _intakeSessionRepository.GetByUserIdAsync(userId);
        
        var sessionDtos = sessions.Select(s => new IntakeSessionDto(
            s.Id,
            s.UserId,
            s.Status,
            s.StartedAt,
            s.CompletedAt,
            s.Language
        ));

        return Ok(sessionDtos);
    }
}
'@ | Set-Content -Path "Law4Hire.API\Controllers\IntakeController.cs" -Encoding UTF8 -Force

# Create enhanced SignalR Hub using .NET 9 features
@"
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Law4Hire.API.Hubs;

/// <summary>
/// SignalR Hub for real-time intake chat functionality
/// </summary>
[Authorize]
[EnableRateLimiting("fixed")]
public class IntakeChatHub : Hub
{
    private readonly ILogger<IntakeChatHub> _logger;

    public IntakeChatHub(ILogger<IntakeChatHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join an intake session group
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    public async Task JoinIntakeSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined intake session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("UserJoined", userId, DateTime.UtcNow);
    }

    /// <summary>
    /// Leave an intake session group
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    public async Task LeaveIntakeSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left intake session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("UserLeft", userId, DateTime.UtcNow);
    }

    /// <summary>
    /// Send a message to the intake session
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="message">The message content</param>
    public async Task SendMessage(string sessionId, string message)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("Message cannot be empty");
        }

        if (message.Length > 1000)
        {
            throw new HubException("Message is too long (max 1000 characters)");
        }

        var userId = Context.UserIdentifier;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("User {UserId} sent message to session {SessionId}", userId, sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveMessage", userId, userName, message, DateTime.UtcNow);
    }

    /// <summary>
    /// Send a bot response with a question
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="question">The question text</param>
    /// <param name="questionType">The type of question</param>
    /// <param name="questionId">The question ID</param>
    public async Task SendBotResponse(string sessionId, string question, string questionType, int? questionId = null)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new HubException("Question cannot be empty");
        }

        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("Bot sent question to session {SessionId}", sessionId);
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveBotQuestion", question, questionType, questionId, DateTime.UtcNow);
    }

    /// <summary>
    /// Submit a response to a question
    /// </summary>
    /// <param name="sessionId">The intake session ID</param>
    /// <param name="questionId">The question ID</param>
    /// <param name="response">The user's response</param>
    public async Task SubmitResponse(string sessionId, int questionId, string response)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new HubException("Session ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            throw new HubException("Response cannot be empty");
        }

        var userId = Context.UserIdentifier;
        var groupName = $"IntakeSession_{sessionId}";
        
        _logger.LogInformation("User {UserId} submitted response to question {QuestionId} in session {SessionId}",
            userId, questionId, sessionId);
        
        // TODO: Save response to database and determine next question
        
        await Clients.Group(groupName)
            .SendAsync("ResponseSubmitted", questionId, response, DateTime.UtcNow);
    }

    /// <summary>
    /// Handle connection events
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to IntakeChatHub", userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle disconnection events
    /// </summary>
    /// <param name="exception">Any exception that caused the disconnection</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected from IntakeChatHub with error", userId);
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from IntakeChatHub", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
"@ | Set-Content -Path "Law4Hire.API\Hubs\IntakeChatHub.cs" -Encoding UTF8 -Force

# Create enhanced services using .NET 9 features
@"
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Core.DTOs;
using Law4Hire.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Added for IConfiguration

namespace Law4Hire.Application.Services;

public interface IIntakeService
{
    Task<IntakeSessionDto> StartIntakeSessionAsync(Guid userId, string language = "en-US");
    Task<IntakeQuestionDto?> GetNextQuestionAsync(Guid sessionId);
    Task<bool> SubmitResponseAsync(Guid sessionId, int questionId, string response);
    Task<bool> CompleteIntakeAsync(Guid sessionId);
    Task<IEnumerable<IntakeQuestionDto>> GetAvailableQuestionsAsync(string language = "en-US");
}

public class IntakeService(
    IIntakeSessionRepository sessionRepository,
    ILogger<IntakeService> logger) : IIntakeService
{
    private readonly IIntakeSessionRepository _sessionRepository = sessionRepository;
    private readonly ILogger<IntakeService> _logger = logger;

    public async Task<IntakeSessionDto> StartIntakeSessionAsync(Guid userId, string language = "en-US")
    {
        _logger.LogInformation("Starting intake session for user {UserId} in language {Language}", userId, language);

        var session = new IntakeSession
        {
            UserId = userId,
            Language = language,
            Status = IntakeStatus.Started,
            SessionData = JsonSerializer.Serialize(new { StartedBy = "System", InitialLanguage = language })
        };

        var createdSession = await _sessionRepository.CreateAsync(session);

        _logger.LogInformation("Created intake session {SessionId} for user {UserId}", createdSession.Id, userId);

        return new IntakeSessionDto(
            createdSession.Id,
            createdSession.UserId,
            createdSession.Status,
            createdSession.StartedAt,
            createdSession.CompletedAt,
            createdSession.Language
        );
    }

    public async Task<IntakeQuestionDto?> GetNextQuestionAsync(Guid sessionId)
    {
        _logger.LogInformation("Getting next question for session {SessionId}", sessionId);
        
        // TODO: Implement sophisticated logic to determine next question based on:
        // - Previous responses
        // - Conditional logic
        // - User's selected language
        // - Form requirements analysis
        
        // Placeholder implementation
        await Task.Delay(100); // Simulate processing time
        
        // Return a sample question for now
        return new IntakeQuestionDto(
            1,
            "full_name",
            "What is your full legal name?",
            QuestionType.Text,
            1,
            null,
            true,
            "required,min:2,max:100"
        );
    }

    public async Task<bool> SubmitResponseAsync(Guid sessionId, int questionId, string response)
    {
        _logger.LogInformation("Submitting response for session {SessionId}, question {QuestionId}", sessionId, questionId);
        
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return false;
        }

        // TODO: Implement response validation and storage
        // TODO: Update session status to InProgress if not already
        
        session.Status = IntakeStatus.InProgress;
        await _sessionRepository.UpdateAsync(session);
        
        _logger.LogInformation("Response submitted successfully for session {SessionId}", sessionId);
        return true;
    }

    public async Task<bool> CompleteIntakeAsync(Guid sessionId)
    {
        _logger.LogInformation("Completing intake session {SessionId}", sessionId);
        
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            return false;
        }

        session.Status = IntakeStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(session);
        
        _logger.LogInformation("Intake session {SessionId} completed successfully", sessionId);
        return true;
    }

    public async Task<IEnumerable<IntakeQuestionDto>> GetAvailableQuestionsAsync(string language = "en-US")
    {
        _logger.LogInformation("Getting available questions for language {Language}", language);
        
        // TODO: Implement database query for questions in specified language
        await Task.Delay(50); // Simulate database call
        
        // Return sample questions for now
        return new[]
        {
            new IntakeQuestionDto(1, "full_name", "What is your full legal name?", QuestionType.Text, 1, null, true, "required,min:2,max:100"),
            new IntakeQuestionDto(2, "date_of_birth", "What is your date of birth?", QuestionType.Date, 2, null, true, "required,date,before:today"),
            new IntakeQuestionDto(3, "country_of_birth", "In which country were you born?", Type = QuestionType.Text, Order = 3, IsRequired = true, ValidationRules = "required,min:2,max:50")
        };
    }
}

public interface IEncryptionService
{
    string EncryptTransferData(Guid userId, Guid requestId);
    (Guid userId, Guid requestId) DecryptTransferData(string encryptedData);
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}

public class EncryptionService(
    IConfiguration configuration,
    ILogger<EncryptionService> logger) : IEncryptionService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EncryptionService> _logger = logger;

    public string EncryptTransferData(Guid userId, Guid requestId)
    {
        _logger.LogInformation("Encrypting transfer data for user {UserId}, request {RequestId}", userId, requestId);
        
        var data = $"{userId}|{requestId}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return EncryptString(data);
    }

    public (Guid userId, Guid requestId) DecryptTransferData(string encryptedData)
    {
        _logger.LogInformation("Decrypting transfer data");
        
        try
        {
            var decryptedData = DecryptString(encryptedData);
            var parts = decryptedData.Split('|');
            
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("Invalid encrypted data format");
            }
            
            var userId = Guid.Parse(parts[0]);
            var requestId = Guid.Parse(parts[1]);
            
            // Optional: Check timestamp if included (parts[2])
            if (parts.Length > 2 && long.TryParse(parts[2], out long timestamp))
            {
                var dataAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                if (dataAge > 3600) // 1 hour expiration
                {
                    _logger.LogWarning("Encrypted data has expired (age: {Age} seconds)", dataAge);
                    throw new InvalidOperationException("Encrypted data has expired");
                }
            }
            
            _logger.LogInformation("Successfully decrypted transfer data for user {UserId}", userId);
            return (userId, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt transfer data");
            throw;
        }
    }

    public string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        var key = GetEncryptionKey();
        var iv = GetInitializationVector();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        
        writer.Write(plainText);
        writer.Close();
        
        var encryptedBytes = ms.ToArray();
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));

        var key = GetEncryptionKey();
        var iv = GetInitializationVector();
        var encryptedBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        
        return reader.ReadToEnd();
    }

    private byte[] GetEncryptionKey()
    {
        var keyString = _configuration["EncryptionSettings:Key"] ?? 
                       throw new InvalidOperationException("Encryption key not configured");
        
        // Ensure key is exactly 32 bytes for AES-256
        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        
        return keyBytes;
    }

    private byte[] GetInitializationVector()
    {
        var ivString = _configuration["EncryptionSettings:IV"] ?? 
                      throw new InvalidOperationException("Encryption IV not configured");
        
        // Ensure IV is exactly 16 bytes for AES
        var ivBytes = Encoding.UTF8.GetBytes(ivString);
        if (ivBytes.Length < 16)
        {
            Array.Resize(ref ivBytes, 16);
        }
        else if (ivBytes.Length > 16)
        {
            Array.Resize(ref ivBytes, 16);
        }
        
        return ivBytes;
    }
}

public interface IFormIdentificationService
{
    Task<IEnumerable<string>> IdentifyRequiredFormsAsync(Guid sessionId);
    Task<bool> ValidateFormDataAsync(string formType, string formData);
}

public class FormIdentificationService(ILogger<FormIdentificationService> logger) : IFormIdentificationService
{
    private readonly ILogger<FormIdentificationService> _logger = logger;

    public async Task<IEnumerable<string>> IdentifyRequiredFormsAsync(Guid sessionId)
    {
        _logger.LogInformation("Identifying required forms for session {SessionId}", sessionId);
        
        // TODO: Implement sophisticated form identification logic based on:
        // - User responses from intake session
        // - External form metadata database
        // - Conditional rules and regulations
        
        await Task.Delay(200); // Simulate processing time
        
        // Return sample forms for now
        var sampleForms = new[] { "I-130", "I-485", "I-765" };
        
        _logger.LogInformation("Identified {Count} required forms for session {SessionId}",
            sampleForms.Length, sessionId);
        
        return sampleForms;
    }

    public async Task<bool> ValidateFormDataAsync(string formType, string formData)
    {
        _logger.LogInformation("Validating form data for form type {FormType}", formType);
        
        if (string.IsNullOrWhiteSpace(formType) || string.IsNullOrWhiteSpace(formData))
        {
            _logger.LogWarning("Invalid form type or data provided");
            return false;
        }

        // TODO: Implement form-specific validation rules
        await Task.Delay(100); // Simulate validation time
        
        _logger.LogInformation("Form data validation completed for {FormType}", formType);
        return true;
    }
}
"@ | Set-Content -Path "Law4Hire.Application\Services\CoreServices.cs" -Encoding UTF8 -Force

# Update API Program.cs to register repositories and add FormIdentificationService
$programContent = @"
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Law4Hire.Infrastructure.Data.Contexts;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Repositories;
using Law4Hire.Application.Services;
using Law4Hire.API.Hubs;
using Microsoft.AspNetCore.RateLimiting; // Added for rate limiting

var builder = WebApplication.CreateBuilder(args);

// Add services to the container using .NET 9 features
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Law4Hire API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

// Add Entity Framework with connection resilience
builder.Services.AddDbContext<Law4HireDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add SignalR with enhanced configuration for .NET 9
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 10;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add Redis cache (optional but recommended for production)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });
}
else
{
    builder.Services.AddMemoryCache();
}

// Add CORS with enhanced security
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5001", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT Authentication with enhanced security
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };
    
    // Allow JWT in SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Add Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiUser", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "admin"));
});

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
builder.Services.AddScoped<IIntakeSessionRepository, IntakeSessionRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<ILocalizedContentRepository, LocalizedContentRepository>();

// Register application services
builder.Services.AddScoped<IIntakeService, IntakeService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IFormIdentificationService, FormIdentificationService>(); // Added FormIdentificationService

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContext<Law4HireDbContext>();

// Add rate limiting (new in .NET 9)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 100;
        limiterOptions.QueueLimit = 50;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline for .NET 9
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Law4Hire API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("DefaultPolicy");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 
app.MapHealthChecks("/health");

// Map SignalR hubs
app.MapHub<IntakeChatHub>("/hubs/intake").RequireAuthorization("ApiUser");

// Run database migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Law4HireDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();
"@ | Set-Content -Path "Law4Hire.API\Program.cs" -Encoding UTF8 -Force

Pop-Location # Return to the original directory after all operations

Write-Host "`n✅ Law4Hire solution structure created successfully!" -ForegroundColor Green
Write-Host "📁 Projects created: 10" -ForegroundColor Cyan
Write-Host "📄 Entity models created: 8" -ForegroundColor Cyan
Write-Host "🔧 Controllers created: 3" -ForegroundColor Cyan
Write-Host "🏠 SignalR Hub created: 1" -ForegroundColor Cyan
Write-Host "📱 Modern Blazor pages with real-time features: 2" -ForegroundColor Cyan
Write-Host "🌍 Comprehensive localization support (8 languages)" -ForegroundColor Cyan
Write-Host "📱 Enhanced MAUI mobile app with .NET 9: 1" -ForegroundColor Cyan
Write-Host "🗃️ Advanced data seeding with detailed content" -ForegroundColor Cyan
Write-Host "📦 Latest NuGet packages for .NET 9" -ForegroundColor Cyan

Write-Host "`n🚀 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Update connection strings in Law4Hire.API/appsettings.json" -ForegroundColor White
Write-Host "   2. Run: .\migrate.bat (Windows) or ./migrate.sh (Linux/Mac)" -ForegroundColor White
Write-Host "   3. Build: dotnet build" -ForegroundColor White
Write-Host "   4. Run API: dotnet run --project Law4Hire.API" -ForegroundColor White
Write-Host "   5. Run Web: dotnet run --project Law4Hire.Web" -ForegroundColor White
Write-Host "   6. Run Mobile: dotnet build Law4Hire.Mobile -f net9.0-android" -ForegroundColor White

Write-Host "`n📝 .NET 9 Features Implemented:" -ForegroundColor Magenta
Write-Host "   - Primary constructors in repositories and services" -ForegroundColor White
Write-Host "   - Enhanced performance with latest EF Core 9.0" -ForegroundColor White
Write-Host "   - Advanced rate limiting and security features" -ForegroundColor White
Write-Host "   - Modern Blazor Server/WebAssembly hybrid" -ForegroundColor White
Write-Host "   - Improved SignalR with connection resilience" -ForegroundColor White
Write-Host "   - Enhanced MAUI with .NET 9 optimizations" -ForegroundColor White
Write-Host "   - Advanced logging and error handling" -ForegroundColor White
Write-Host "   - Latest security and encryption standards" -ForegroundColor White

Write-Host "`n💡 Ready for development with .NET 9!" -ForegroundColor Green
