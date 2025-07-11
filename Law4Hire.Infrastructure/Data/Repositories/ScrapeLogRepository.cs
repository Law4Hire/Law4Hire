using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.Infrastructure.Data.Repositories;

public class ScrapeLogRepository(Law4HireDbContext context) : IScrapeLogRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task AddAsync(ScrapeLog log)
    {
        _context.ScrapeLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
