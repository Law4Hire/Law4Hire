using System.Text.RegularExpressions;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlaywrightSharp;

namespace Law4Hire.Scraper;

public class VisaScraperBot(
    IServiceScopeFactory scopeFactory,
    ILogger<VisaScraperBot> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<VisaScraperBot> _logger = logger;
    private const string SourceUrl = "https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/all-visa-categories.html";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var scraped = await ScrapeVisaTypesAsync(stoppingToken);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Law4HireDbContext>();
            var logRepo = scope.ServiceProvider.GetRequiredService<IScrapeLogRepository>();

            foreach (var visa in scraped)
            {
                var exists = await db.VisaTypes.FirstOrDefaultAsync(v => v.Name == visa.Name, cancellationToken: stoppingToken);
                if (exists == null)
                {
                    db.VisaTypes.Add(visa);
                    await db.SaveChangesAsync(stoppingToken);
                    await logRepo.AddAsync(new ScrapeLog
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Action = "Created",
                        EntityAffected = visa.Name,
                        Notes = SourceUrl
                    });
                }
                else
                {
                    await logRepo.AddAsync(new ScrapeLog
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Action = "Skipped",
                        EntityAffected = exists.Name,
                        Notes = "Already exists"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            using var scope = _scopeFactory.CreateScope();
            var logRepo = scope.ServiceProvider.GetRequiredService<IScrapeLogRepository>();
            await logRepo.AddAsync(new ScrapeLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Action = "Error",
                EntityAffected = "VisaScraperBot",
                Notes = ex.Message
            });
            _logger.LogError(ex, "Scraper failed");
        }
    }

    private static async Task<List<VisaType>> ScrapeVisaTypesAsync(CancellationToken token)
    {
        var list = new List<VisaType>();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new LaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GoToAsync(SourceUrl);
        var rows = await page.QuerySelectorAllAsync("table.grid tbody tr");
        var map = new Dictionary<string, Guid>
        {
            {"B-1", Guid.Parse("11111111-1111-1111-1111-111111111111")},
            {"B-2", Guid.Parse("11111111-1111-1111-1111-111111111111")},
            {"J", Guid.Parse("66666666-6666-6666-6666-666666666666")},
            {"O", Guid.Parse("44444444-4444-4444-4444-444444444444")}
        };

        foreach (var row in rows)
        {
            var cells = await row.QuerySelectorAllAsync("td");
            if (cells.Length < 2) continue;
            var desc = (await (await cells[0].GetPropertyAsync("innerText")).JsonValueAsync<string>()).Trim();
            var visaName = (await (await cells[1].GetPropertyAsync("innerText")).JsonValueAsync<string>()).Trim();
            visaName = Regex.Replace(visaName, "\\s+", " ");
            if (string.IsNullOrWhiteSpace(visaName)) continue;
            list.Add(new VisaType
            {
                Id = Guid.NewGuid(),
                Name = visaName,
                Description = desc,
                Category = "Unknown",
                VisaGroupId = map.TryGetValue(visaName, out var gid) ? gid : Guid.Parse("11111111-1111-1111-1111-111111111111")
            });
        }

        return list;
    }
}
