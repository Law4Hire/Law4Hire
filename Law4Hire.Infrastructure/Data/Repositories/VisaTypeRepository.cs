using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.Infrastructure.Data.Repositories;

public class VisaTypeRepository(Law4HireDbContext context) : IVisaTypeRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IEnumerable<VisaType>> GetAllAsync()
    {
        return await _context.Set<VisaType>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<VisaType>> GetByCategoryAsync(string category)
    {
        return await _context.Set<VisaType>()
            .Where(v => v.Category == category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VisaType?> GetByIdAsync(Guid id)
    {
        return await _context.Set<VisaType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}
