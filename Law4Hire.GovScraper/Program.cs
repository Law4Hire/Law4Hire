using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Law4Hire.GovScraper;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Build service collection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add Entity Framework
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=Law4Hire;Trusted_Connection=True;TrustServerCertificate=True;";
        
        services.AddDbContext<Law4HireDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add scraper services
        services.AddTransient<CountryScraper>();
        services.AddTransient<VisaWizardTester>();
        services.AddTransient<TestDataChecker>();
        services.AddTransient<TestDataGenerator>();
        services.AddTransient<WebsiteAnalyzer>();
        services.AddTransient<StaticVisaWizardScraper>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();
        
        // Get logger
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Ensure database is created
            using var dbContext = serviceProvider.GetRequiredService<Law4HireDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("Database connection verified");

            // Check command line arguments
            string mode = args.Length > 0 ? args[0].ToLower() : "wizard";
            //string mode = args.Length > 0 ? args[0].ToLower() : "countries";

            switch (mode)
            {
                case "countries":
                    logger.LogInformation("🌍 STARTING LAW4HIRE COUNTRY SCRAPER 🌍");
                    logger.LogInformation($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logger.LogInformation("🌐 Target: travel.state.gov visa wizard autocomplete");
                    logger.LogInformation("📍 Mode: COUNTRY COLLECTION (A-Z autocomplete scraping)");
                    logger.LogInformation("💾 Database: localhost\\Law4Hire\\Countries table");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var countryScraper = serviceProvider.GetRequiredService<CountryScraper>();
                    var countries = await countryScraper.ScrapeAllCountriesAsync();

                    logger.LogInformation("🎉 COUNTRY SCRAPING COMPLETED SUCCESSFULLY! 🎉");
                    logger.LogInformation($"📊 COLLECTED {countries.Count} COUNTRIES:");
                    logger.LogInformation("✅ Database: Countries table updated");
                    logger.LogInformation("🔍 Sample countries: " + string.Join(", ", countries.Take(10)));
                    if (countries.Count > 10)
                        logger.LogInformation($"... and {countries.Count - 10} more countries");
                    break;
                    
                case "test-countries":
                    logger.LogInformation("🧪 TESTING COUNTRY DATABASE INTEGRATION 🧪");
                    logger.LogInformation($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logger.LogInformation("📍 Mode: TEST MODE (populating sample countries)");
                    logger.LogInformation("💾 Database: localhost\\Law4Hire\\Countries table");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var testCountryScraper = serviceProvider.GetRequiredService<CountryScraper>();
                    var testCountries = await testCountryScraper.TestPopulateCountriesAsync();

                    logger.LogInformation("🎉 TEST COUNTRY POPULATION COMPLETED! 🎉");
                    logger.LogInformation($"📊 POPULATED {testCountries.Count} TEST COUNTRIES:");
                    logger.LogInformation("✅ Database: Countries table populated with test data");
                    logger.LogInformation("🔍 Countries: " + string.Join(", ", testCountries));
                    logger.LogInformation("🔄 Now you can test the visa wizard with: dotnet run test-wizard");
                    break;
                    
                case "test-wizard":
                    logger.LogInformation("🧪 STARTING LAW4HIRE VISA WIZARD TESTER 🧪");
                    logger.LogInformation($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logger.LogInformation("🌐 Target: travel.state.gov visa wizard");
                    logger.LogInformation("📍 Mode: SYSTEMATIC TESTING (5 countries, all purposes & questions)");
                    logger.LogInformation("💾 Database: localhost\\Law4Hire\\VisaWizards table");
                    logger.LogInformation("🎯 Testing all combinations systematically");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var wizardTester = serviceProvider.GetRequiredService<VisaWizardTester>();
                    await wizardTester.TestVisaWizardAsync(5); // Test first 5 countries
                    
                    logger.LogInformation("🎉 VISA WIZARD TESTING COMPLETED! 🎉");
                    logger.LogInformation("✅ Database: Results saved to VisaWizards table");
                    logger.LogInformation("🔍 View results: SELECT * FROM VisaWizards ORDER BY Country, Purpose, StepNumber");
                    break;
                    
                case "test-5-countries":
                    {
                        logger.LogInformation("🧪 STARTING LAW4HIRE VISA WIZARD TESTER - 5 SPECIFIC COUNTRIES 🧪");
                        logger.LogInformation($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        logger.LogInformation("🌐 Target: travel.state.gov visa wizard");
                        logger.LogInformation("📍 Mode: SPECIFIC TESTING (First 4 countries + Canada)");
                        logger.LogInformation("💾 Database: localhost\\Law4Hire\\VisaWizards table");
                        logger.LogInformation("🗑️ Will clear existing data and start fresh");
                        logger.LogInformation("=" + new string('=', 60));
                        
                        var specificTester = serviceProvider.GetRequiredService<VisaWizardTester>();
                        
                        // Get first 4 countries from database + Canada
                        using var specificDbContext = serviceProvider.GetRequiredService<Law4HireDbContext>();
                        var first4Countries = await specificDbContext.Countries
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.SortOrder)
                            .Take(4)
                            .Select(c => c.Name)
                            .ToListAsync();
                        
                        var specificTestCountries = new List<string>(first4Countries);
                        if (!specificTestCountries.Contains("Canada"))
                        {
                            specificTestCountries.Add("Canada");
                        }
                        
                        logger.LogInformation($"🎯 Testing countries: {string.Join(", ", specificTestCountries)}");
                        
                        await specificTester.TestSpecificCountriesAsync(specificTestCountries);
                        
                        logger.LogInformation("🎉 5-COUNTRY TESTING COMPLETED! 🎉");
                        logger.LogInformation("✅ Database: Results saved to VisaWizards table");
                        logger.LogInformation("🔍 View results: SELECT * FROM VisaWizards ORDER BY Country, Purpose, StepNumber");
                        break;
                    }
                    
                case "check-results":
                    logger.LogInformation("🔍 CHECKING VISA WIZARD RESULTS IN DATABASE");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var dataChecker = serviceProvider.GetRequiredService<TestDataChecker>();
                    await dataChecker.CheckVisaWizardResultsAsync();
                    break;
                    
                case "analyze-website":
                    logger.LogInformation("🔍 ANALYZING VISA WIZARD WEBSITE STRUCTURE");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var websiteAnalyzer = serviceProvider.GetRequiredService<WebsiteAnalyzer>();
                    await websiteAnalyzer.AnalyzeVisaWizardStructureAsync();
                    break;
                    
                case "static-scraper":
                    logger.LogInformation("🚀 STARTING STATIC VISA WIZARD SCRAPER");
                    logger.LogInformation("🎯 DETERMINISTIC APPROACH - NO GUESSING");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var staticScraper = serviceProvider.GetRequiredService<StaticVisaWizardScraper>();
                    await staticScraper.ScrapeAllCountriesStaticAsync();
                    break;
                    
                case "static-test":
                    logger.LogInformation("🧪 TESTING STATIC VISA WIZARD SCRAPER");
                    logger.LogInformation("🎯 TESTING WITH 3 COUNTRIES");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var staticTestScraper = serviceProvider.GetRequiredService<StaticVisaWizardScraper>();
                    await staticTestScraper.TestStaticApproachAsync();
                    break;
                    
                case "wizard":
                default:
                {
                    logger.LogInformation("Starting Law4Hire Government Visa Wizard Comprehensive Collection");
                    logger.LogInformation("This will collect all visa wizard questions and answers from travel.state.gov with enhanced systematic approach");
                    
                    logger.LogInformation("🚀 STARTING LAW4HIRE COMPREHENSIVE VISA WIZARD COLLECTION 🚀");
                    logger.LogInformation($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    logger.LogInformation("🌐 Target: travel.state.gov visa wizard");
                    logger.LogInformation("📍 Mode: COMPREHENSIVE COLLECTION (all 214 countries, all purposes, all questions)");
                    logger.LogInformation("💾 Database: localhost\\Law4Hire\\VisaWizards table");
                    logger.LogInformation("🎯 Using enhanced systematic exploration with proper form submission");
                    logger.LogInformation("🔄 Browser refreshes every 3 countries for stability");
                    logger.LogInformation("♻️ Automatic retry logic for failed countries");
                    logger.LogInformation("⚡ Designed for overnight collection of comprehensive visa data");
                    logger.LogInformation("=" + new string('=', 60));
                    
                    var comprehensiveTester = serviceProvider.GetRequiredService<VisaWizardTester>();
                    
                    // Clear existing data first
                    using var clearDbContext = serviceProvider.GetRequiredService<Law4HireDbContext>();
                    var existingCount = await clearDbContext.VisaWizards.CountAsync();
                    if (existingCount > 0)
                    {
                        logger.LogInformation($"🗑️ Clearing {existingCount} existing VisaWizard records...");
                        clearDbContext.VisaWizards.RemoveRange(clearDbContext.VisaWizards);
                        await clearDbContext.SaveChangesAsync();
                        logger.LogInformation("✅ Database cleared, starting fresh");
                    }
                    
                    // Get all countries from database
                    var allCountries = await clearDbContext.Countries
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.SortOrder)
                        .Select(c => c.Name)
                        .ToListAsync();
                    
                    logger.LogInformation($"📊 Will process {allCountries.Count} countries comprehensively");
                    
                    // Process all countries using the working systematic approach
                    await comprehensiveTester.TestSpecificCountriesAsync(allCountries);
                    
                    logger.LogInformation("🎉 COMPREHENSIVE VISA WIZARD COLLECTION COMPLETED SUCCESSFULLY! 🎉");
                    logger.LogInformation("");
                    logger.LogInformation("📊 DATA SAVED TO:");
                    logger.LogInformation("✅ Database: VisaWizards table (localhost\\Law4Hire)");
                    logger.LogInformation("");
                    logger.LogInformation("🔍 TO VIEW RESULTS:");
                    logger.LogInformation("   SQL: SELECT * FROM VisaWizards ORDER BY Country, Purpose");
                    logger.LogInformation("   Or run: dotnet run check-results");
                    logger.LogInformation("");
                    logger.LogInformation("✨ COMPREHENSIVE COLLECTION COMPLETE - ALL COUNTRIES PROCESSED! ✨");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during scraping process");
            Environment.Exit(1);
        }
        finally
        {
            serviceProvider.Dispose();
        }

        if (Environment.UserInteractive && !Console.IsInputRedirected)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
