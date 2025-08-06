using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Logging;

namespace Law4Hire.GovScraper;

public class TestDataGenerator
{
    private readonly ILogger<TestDataGenerator> _logger;
    private readonly Law4HireDbContext _dbContext;

    public TestDataGenerator(ILogger<TestDataGenerator> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task GenerateTestDataAsync()
    {
        _logger.LogInformation("Generating test visa wizard data for demonstration");

        var testData = new List<VisaWizard>
        {
            new VisaWizard
            {
                Country = "Canada",
                Purpose = "Business",
                Answer1 = "Attending meetings and conferences",
                StepNumber = 1,
                IsCompleteSession = false,
                LearnMoreLinks = "https://travel.state.gov/content/travel/en/us-visas/business.html",
                RelatedVisaCategories = "B-1 Business Visitor"
            },
            new VisaWizard
            {
                Country = "Canada", 
                Purpose = "Business",
                Answer1 = "Attending meetings and conferences",
                Answer2 = "Less than 90 days",
                StepNumber = 2,
                IsCompleteSession = false
            },
            new VisaWizard
            {
                Country = "Canada",
                Purpose = "Business", 
                Answer1 = "Attending meetings and conferences",
                Answer2 = "Less than 90 days",
                StepNumber = 3,
                IsCompleteSession = true,
                OutcomeDisplayContent = "Based on your responses, you may be eligible for a B-1 business visitor visa. This allows temporary business activities in the US.",
                LearnMoreLinks = "https://travel.state.gov/content/travel/en/us-visas/business.html",
                RelatedVisaCategories = "B-1 Business Visitor, ESTA"
            },
            new VisaWizard
            {
                Country = "United Kingdom",
                Purpose = "Tourism",
                Answer1 = "Vacation and sightseeing",
                StepNumber = 1,
                IsCompleteSession = false
            },
            new VisaWizard
            {
                Country = "United Kingdom",
                Purpose = "Tourism",
                Answer1 = "Vacation and sightseeing",
                Answer2 = "2 weeks",
                StepNumber = 2,
                IsCompleteSession = false
            },
            new VisaWizard
            {
                Country = "United Kingdom",
                Purpose = "Tourism",
                Answer1 = "Vacation and sightseeing",
                Answer2 = "2 weeks",
                StepNumber = 3,
                IsCompleteSession = true,
                OutcomeDisplayContent = "UK citizens can travel to the US for tourism under the Visa Waiver Program using ESTA authorization.",
                LearnMoreLinks = "https://esta.cbp.dhs.gov/",
                RelatedVisaCategories = "ESTA, B-2 Tourist"
            },
            new VisaWizard
            {
                Country = "India",
                Purpose = "Work",
                Answer1 = "Yes, I have an approved H-1B petition",
                StepNumber = 1,
                IsCompleteSession = false
            },
            new VisaWizard
            {
                Country = "India", 
                Purpose = "Work",
                Answer1 = "Yes, I have an approved H-1B petition",
                Answer2 = "Specialty occupation in technology",
                StepNumber = 2,
                IsCompleteSession = false
            },
            new VisaWizard
            {
                Country = "India",
                Purpose = "Work",
                Answer1 = "Yes, I have an approved H-1B petition",
                Answer2 = "Specialty occupation in technology",
                StepNumber = 3, 
                IsCompleteSession = true,
                OutcomeDisplayContent = "Based on your approved petition, you can apply for an H-1B visa to work temporarily in the US in a specialty occupation.",
                LearnMoreLinks = "https://travel.state.gov/content/travel/en/us-visas/employment/temporary-worker-visas.html",
                RelatedVisaCategories = "H-1B, L-1, O-1"
            }
        };

        try
        {
            await _dbContext.VisaWizards.AddRangeAsync(testData);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Successfully saved {testData.Count} test visa wizard entries to database");
            
            // Save to text file as well
            await SaveToTextFileAsync(testData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test visa wizard data");
            throw;
        }
    }

    private async Task SaveToTextFileAsync(List<VisaWizard> data)
    {
        var govScriptsDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "GovScripts");
        if (!Directory.Exists(govScriptsDir))
        {
            Directory.CreateDirectory(govScriptsDir);
        }

        var filePath = Path.Combine(govScriptsDir, "test_visa_wizard_data.txt");
        
        using var writer = new StreamWriter(filePath);
        await writer.WriteLineAsync("=== Test Visa Wizard Data ===");
        await writer.WriteLineAsync($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await writer.WriteLineAsync();

        foreach (var entry in data)
        {
            await writer.WriteLineAsync($"Country: {entry.Country}");
            await writer.WriteLineAsync($"Purpose: {entry.Purpose}");
            await writer.WriteLineAsync($"Step {entry.StepNumber}");
            {
                await writer.WriteLineAsync($"A1: {entry.Answer1}");
            }
            {
                await writer.WriteLineAsync($"A2: {entry.Answer2}");
            }
            
            if (!string.IsNullOrEmpty(entry.LearnMoreLinks))
                await writer.WriteLineAsync($"Learn More: {entry.LearnMoreLinks}");
            
            if (!string.IsNullOrEmpty(entry.RelatedVisaCategories))
                await writer.WriteLineAsync($"Related Visas: {entry.RelatedVisaCategories}");
                
            if (!string.IsNullOrEmpty(entry.OutcomeDisplayContent))
                await writer.WriteLineAsync($"Outcome: {entry.OutcomeDisplayContent}");
                
            await writer.WriteLineAsync($"Complete Session: {entry.IsCompleteSession}");
            await writer.WriteLineAsync("---");
        }

        _logger.LogInformation($"Test data saved to text file: {filePath}");
    }
}