using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Law4Hire.Infrastructure.Data.Repositories;
using Law4Hire.Scraper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IBruceOpenAIAgent, BruceOpenAIAgent>();
        services.AddScoped<IVisaSyncService, VisaSyncService>();
        services.AddHostedService<BruceVisaBot>();
        services.AddScoped<IScrapeLogRepository, ScrapeLogRepository>();

    })
    .Build();

await host.RunAsync();
public class Law4HireDbContext : DbContext
{
    public Law4HireDbContext(DbContextOptions<Law4HireDbContext> options)
        : base(options)
    {
    }

    // DbSets...
}
