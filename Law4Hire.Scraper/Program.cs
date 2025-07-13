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
        var connection = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<Law4HireDbContext>(options => options.UseSqlServer(connection));
        services.AddScoped<IVisaTypeRepository, VisaTypeRepository>();
        services.AddScoped<IScrapeLogRepository, ScrapeLogRepository>();
        services.AddHostedService<VisaScraperBot>();
    })
    .Build();

await host.RunAsync();
