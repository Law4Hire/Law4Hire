using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Law4Hire.GovScraper;

public class TestDataChecker
{
    private readonly ILogger<TestDataChecker> _logger;
    private readonly Law4HireDbContext _dbContext;

    public TestDataChecker(ILogger<TestDataChecker> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task CheckVisaWizardResultsAsync()
    {
        try
        {
            var results = await _dbContext.VisaWizards
                .OrderBy(vw => vw.CreatedAt)
                .Select(vw => new
                {
                    vw.Country,
                    vw.Purpose,
                    vw.Answer1,
                    vw.Answer2,
                    vw.LearnMoreLinks,
                    vw.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation($"📊 Found {results.Count} visa wizard results in database:");
            
            foreach (var result in results)
            {
                var a1 = result.Answer1 ?? "None";
                var a2 = result.Answer2 ?? "None";
                
                _logger.LogInformation($"  🌍 {result.Country} | {result.Purpose}");
                _logger.LogInformation($"     A1: '{a1}' | A2: '{a2}'");
                _logger.LogInformation($"     Links: {result.LearnMoreLinks?.Split(',').Length ?? 0} | {result.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                _logger.LogInformation($"     ---");
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No visa wizard results found in database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking visa wizard results");
        }
    }
}