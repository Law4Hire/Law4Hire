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

    public async Task UpsertRangeAsync(IEnumerable<VisaType> visaTypes)
    {
        foreach (var visa in visaTypes)
        {
            var existing = await _context.Set<VisaType>()
                .FirstOrDefaultAsync(v => v.Id == visa.Id);

            if (existing == null)
            {
                _context.Set<VisaType>().Add(visa);
            }
            else
            {
                existing.Name = visa.Name;
                existing.Description = visa.Description;
                existing.Category = visa.Category;
                _context.Set<VisaType>().Update(existing);
            }
        }

        await _context.SaveChangesAsync();
    }
}
