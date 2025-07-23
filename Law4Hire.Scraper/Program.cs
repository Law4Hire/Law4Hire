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
        // ADD THIS: Register the DbContext with connection string
        services.AddDbContext<Law4HireDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBruceOpenAIAgent, BruceOpenAIAgent>();
        services.AddScoped<IVisaSyncService, VisaSyncService>();
        services.AddHostedService<BruceVisaBot>();
        services.AddScoped<IScrapeLogRepository, ScrapeLogRepository>();
    })
    .Build();

await host.RunAsync();