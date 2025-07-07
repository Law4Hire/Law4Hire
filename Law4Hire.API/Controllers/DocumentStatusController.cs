using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Law4Hire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentStatusController(Law4HireDbContext context) : ControllerBase
{
    private readonly Law4HireDbContext _context = context;

    // 1. Get all required documents and statuses for a user
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserDocuments(Guid userId)
    {
        var documents = await _context.UserDocumentStatuses
            .Include(s => s.DocumentType)
            .Include(s => s.VisaType)
            .Where(s => s.UserId == userId)
            .Select(s => new
            {
                s.DocumentTypeId,
                DocumentName = s.DocumentType.Name,
                Status = s.Status.ToString(),
                VisaType = s.VisaType.Name
            })
            .ToListAsync();

        return Ok(documents);
    }

    // 2. Update a user's document status (restricted by role)
    [HttpPatch("user/{userId:guid}/document/{documentId:int}")]
    [Authorize(Roles = "AI,LegalProfessional")]
    public async Task<IActionResult> UpdateStatus(Guid userId, Guid documentId, [FromBody] StatusUpdateDto update)
    {
        var statusRecord = await _context.UserDocumentStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentTypeId == documentId);

        if (statusRecord == null)
            return NotFound("Document status record not found.");

        if (!Enum.TryParse<DocumentStatusEnum>(update.Status, true, out var newStatus))
            return BadRequest("Invalid status value.");

        statusRecord.Status = newStatus;
        statusRecord.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Document status updated." });
    }

    // 3. (Optional) AI can seed required documents for a visa type
    [HttpPost("seed/user/{userId:guid}/visa/{visaTypeId:guid}")]
    [Authorize(Roles = "AI,LegalProfessional")]
    public async Task<IActionResult> SeedUserDocuments(Guid userId, Guid visaTypeId)
    {
        var requiredDocs = await _context.VisaDocumentRequirements
            .Where(v => v.VisaTypeId == visaTypeId)
            .Select(v => v.DocumentTypeId)
            .ToListAsync();

        var existing = await _context.UserDocumentStatuses
            .Where(u => u.UserId == userId && u.VisaTypeId == visaTypeId)
            .Select(u => u.DocumentTypeId)
            .ToListAsync();

        var newDocs = requiredDocs.Except(existing).ToList();

        foreach (var docId in newDocs)
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
        return Ok(new { Message = "User document requirements seeded." });
    }
    public class StatusUpdateDto
    {
        /// <example>Accepts: NotStarted, InProgress, Complete</example>
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
