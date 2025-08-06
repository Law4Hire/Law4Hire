using System.Text.Json;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Infrastructure.Data;

public class DataLoader
{
    private readonly Law4HireDbContext _context;
    private readonly ILogger<DataLoader> _logger;

    public DataLoader(Law4HireDbContext context, ILogger<DataLoader> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LoadVisaDataFromUpdatedJsonAsync()
    {
        try
        {
            _logger.LogInformation("Starting to load visa data from Updated.json...");

            // Check if data already exists
            if (await _context.BaseVisaTypes.AnyAsync())
            {
                _logger.LogInformation("BaseVisaTypes already contains data, skipping load.");
                return;
            }

            // Read the Updated.json file
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Updated.json");
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("Updated.json file not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var visaData = JsonSerializer.Deserialize<List<VisaTypeDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (visaData == null || !visaData.Any())
            {
                _logger.LogWarning("No visa data found in Updated.json");
                return;
            }

            var visaTypes = new List<BaseVisaType>();
            foreach (var visa in visaData)
            {
                var visaType = new BaseVisaType
                {
                    Id = Guid.NewGuid(),
                    Code = visa.Code,
                    Name = visa.Name,
                    Description = visa.Description,
                    Question1 = visa.Question1,
                    Question2 = visa.Question2,
                    Question3 = visa.Question3,
                    Status = "Active", // Default to Active since this is new data
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ConfidenceScore = 1.0m
                };
                
                visaTypes.Add(visaType);
            }

            await _context.BaseVisaTypes.AddRangeAsync(visaTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully loaded {Count} visa types from Updated.json", visaTypes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading visa data from Updated.json");
            throw;
        }
    }

    public async Task LoadCategoryClassDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting to load category class data...");

            // Check if data already exists
            if (await _context.CategoryClasses.AnyAsync())
            {
                _logger.LogInformation("CategoryClasses already contains data, skipping load.");
                return;
            }

            // Read the CategoryClass.json file
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "CategoryClass.json");
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("CategoryClass.json file not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var categoryData = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (categoryData == null || !categoryData.Any())
            {
                _logger.LogWarning("No category class data found in CategoryClass.json");
                return;
            }

            var categoryClasses = new List<CategoryClass>();
            foreach (var kvp in categoryData)
            {
                var classCode = kvp.Key;
                var categories = string.Join(", ", kvp.Value);
                
                var categoryClass = new CategoryClass
                {
                    Id = Guid.NewGuid(),
                    ClassCode = classCode,
                    ClassName = GetClassNameFromCode(classCode),
                    Description = $"Visa class for {classCode} category visas",
                    GeneralCategory = categories,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                categoryClasses.Add(categoryClass);
            }

            await _context.CategoryClasses.AddRangeAsync(categoryClasses);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully loaded {Count} category classes", categoryClasses.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category class data");
            throw;
        }
    }

    private static string GetClassNameFromCode(string classCode)
    {
        return classCode switch
        {
            "A" => "Diplomat/Official",
            "B" => "Visitor",
            "C" => "Transit",
            "D" => "Crewmember", 
            "E" => "Treaty Trader/Investor",
            "EB" => "Employment-Based Immigrant",
            "F" => "Student",
            "G" => "International Organization",
            "H" => "Temporary Worker",
            "I" => "Media",
            "IR" => "Immediate Relative",
            "CR" => "Conditional Resident",
            "J" => "Exchange Visitor",
            "K" => "Fiancé(e)/Spouse",
            "L" => "Intracompany Transferee",
            "M" => "Vocational Student",
            "NATO" => "NATO",
            "O" => "Extraordinary Ability",
            "P" => "Artist/Athlete",
            "Q" => "Cultural Exchange",
            "R" => "Religious Worker",
            "S" => "Informant",
            "T" => "Trafficking Victim",
            "TN" => "NAFTA Professional",
            "U" => "Crime Victim",
            "V" => "Spouse/Child of LPR",
            _ => classCode + " Visa Class"
        };
    }
}

public class VisaTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Question1 { get; set; }
    public string? Question2 { get; set; }
    public string? Question3 { get; set; }
}