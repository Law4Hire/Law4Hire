using System.Text.RegularExpressions;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Scraper;

public class BruceVisaBot(
    IServiceScopeFactory scopeFactory,
    ILogger<BruceVisaBot> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<BruceVisaBot> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IVisaSyncService>();

        try
        {
            await syncService.SyncCategoriesAndTypesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            var logRepo = scope.ServiceProvider.GetRequiredService<IScrapeLogRepository>();
            await logRepo.AddAsync(new ScrapeLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Action = "Error",
                EntityAffected = "BruceVisaBot",
                Notes = ex.ToString()
            });
            _logger.LogError(ex, "BruceVisaBot failed");
        }
    }
}