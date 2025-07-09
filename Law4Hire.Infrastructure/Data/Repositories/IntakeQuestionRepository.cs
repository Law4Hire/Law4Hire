using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.Infrastructure.Data.Repositories;

public class IntakeQuestionRepository(Law4HireDbContext context) : IIntakeQuestionRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IEnumerable<IntakeQuestion>> GetByCategoryAsync(string category)
    {
        return await _context.IntakeQuestions
            .Where(q => q.Category == category)
            .OrderBy(q => q.Order)
            .AsNoTracking()
            .ToListAsync();
    }
}
