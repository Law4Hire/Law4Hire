using Law4Hire.Core.Entities;

namespace Law4Hire.Core.Interfaces;

public interface IScrapeLogRepository
{
    Task AddAsync(ScrapeLog log);
}
