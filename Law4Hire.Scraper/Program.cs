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
        // Get the connection string from configuration
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        // Register the DbContext with connection string
        services.AddDbContext<Law4HireDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);

                // Set the migrations assembly to the API project where migrations are located
                sqlOptions.MigrationsAssembly("Law4Hire.API");
            }));

        services.AddScoped<IBruceOpenAIAgent, BruceOpenAIAgent>();
        services.AddScoped<IVisaSyncService, VisaSyncService>();
        services.AddHostedService<BruceVisaBot>();
        services.AddScoped<IScrapeLogRepository, ScrapeLogRepository>();
    })
    .Build();

await host.RunAsync();