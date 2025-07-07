using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Law4Hire.Application.Services;

public class DocumentStatusService(Law4HireDbContext context)
{
    private readonly Law4HireDbContext _context = context;

    public async Task<List<UserDocumentStatus>> GetStatusesAsync(Guid userId)
    {
        return await _context.UserDocumentStatuses
            .Include(x => x.DocumentType)
            .Include(x => x.VisaType)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatus(Guid userId, Guid documentId, DocumentStatusEnum newStatus)
    {
        var item = await _context.UserDocumentStatuses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DocumentTypeId == documentId);

        if (item == null) return false;

        item.Status = newStatus;
        item.LastModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SeedDocumentsForUser(Guid userId, Guid visaTypeId)
    {
        var required = await _context.VisaDocumentRequirements
            .Where(x => x.VisaTypeId == visaTypeId)
            .Select(x => x.DocumentTypeId)
            .ToListAsync();

        var existing = await _context.UserDocumentStatuses
            .Where(x => x.UserId == userId && x.VisaTypeId == visaTypeId)
            .Select(x => x.DocumentTypeId)
            .ToListAsync();

        var toAdd = required.Except(existing);

        foreach (var docId in toAdd)
        {
            _context.UserDocumentStatuses.Add(new UserDocumentStatus
            {
                UserId = userId,
                DocumentTypeId = docId,
                VisaTypeId = visaTypeId,
                Status = DocumentStatusEnum.NotStarted,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
