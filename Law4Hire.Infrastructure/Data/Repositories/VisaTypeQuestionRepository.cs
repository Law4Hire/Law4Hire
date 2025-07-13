using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Law4Hire.Infrastructure.Data.Repositories;

public class VisaTypeQuestionRepository(Law4HireDbContext context) : IVisaTypeQuestionRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IEnumerable<VisaTypeQuestion>> GetByVisaTypeIdAsync(Guid visaTypeId)
    {
        return await _context.Set<VisaTypeQuestion>()
            .Where(q => q.VisaTypeId == visaTypeId)
            .OrderBy(q => q.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpsertRangeAsync(IEnumerable<VisaTypeQuestion> questions)
    {
        foreach (var question in questions)
        {
            var existing = await _context.Set<VisaTypeQuestion>()
                .FirstOrDefaultAsync(q => q.Id == question.Id);

            if (existing == null)
            {
                _context.Set<VisaTypeQuestion>().Add(question);
            }
            else
            {
                existing.QuestionKey = question.QuestionKey;
                existing.QuestionText = question.QuestionText;
                existing.Type = question.Type;
                existing.Order = question.Order;
                existing.IsRequired = question.IsRequired;
                existing.ValidationRules = question.ValidationRules;
                existing.VisaTypeId = question.VisaTypeId;
                _context.Set<VisaTypeQuestion>().Update(existing);
            }
        }

        await _context.SaveChangesAsync();
    }
}
