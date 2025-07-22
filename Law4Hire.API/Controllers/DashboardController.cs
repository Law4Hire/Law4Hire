// Create or update your DashboardController

using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly Law4HireDbContext _context;

    public DashboardController(Law4HireDbContext context)
    {
        _context = context;
    }

    [HttpGet("workflow/{userId:guid}")]
    public async Task<ActionResult<DashboardWorkflowDto>> GetUserWorkflow(Guid userId)
    {
        // Get the user's completed interview state
        var interviewState = await _context.VisaInterviewStates
            .FirstOrDefaultAsync(vis => vis.UserId == userId && vis.IsCompleted);

        if (interviewState == null || string.IsNullOrEmpty(interviewState.VisaWorkflowJson))
        {
            return NotFound("No completed workflow found for user");
        }

        // Parse the workflow JSON
        using var doc = JsonDocument.Parse(interviewState.VisaWorkflowJson);
        var root = doc.RootElement;

        var workflow = new DashboardWorkflowDto
        {
            VisaType = interviewState.SelectedVisaType ?? "Unknown",
            EstimatedTotalCost = root.TryGetProperty("estimatedTotalCost", out var totalCost)
                ? totalCost.GetDecimal() : 0,
            EstimatedTotalTimeDays = root.TryGetProperty("estimatedTotalTimeDays", out var totalTime)
                ? totalTime.GetInt32() : 0
        };

        // Get all document statuses for this user
        var documentStatuses = await _context.UserDocumentStatuses
            .Include(uds => uds.DocumentType)
            .Where(uds => uds.UserId == userId)
            .ToListAsync();

        if (root.TryGetProperty("steps", out var steps))
        {
            foreach (var step in steps.EnumerateArray())
            {
                var stepDto = new WorkflowStepDto
                {
                    Name = step.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                    Description = step.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    EstimatedCost = step.TryGetProperty("estimatedCost", out var cost) ? cost.GetDecimal() : 0,
                    EstimatedTimeDays = step.TryGetProperty("estimatedTimeDays", out var time) ? time.GetInt32() : 0
                };

                // Add government link if this step has one
                if (step.TryGetProperty("link", out var link))
                {
                    stepDto.GovernmentLink = link.GetString();
                }

                // Process documents for this step
                if (step.TryGetProperty("documents", out var documents))
                {
                    foreach (var document in documents.EnumerateArray())
                    {
                        var documentName = document.GetString();
                        if (string.IsNullOrWhiteSpace(documentName)) continue;

                        // Find corresponding document status
                        var docStatus = documentStatuses.FirstOrDefault(ds =>
                            ds.DocumentType.Name.Equals(documentName, StringComparison.OrdinalIgnoreCase));

                        stepDto.Documents.Add(new WorkflowDocumentDto
                        {
                            Name = documentName,
                            Status = docStatus?.Status ?? DocumentStatusEnum.NotStarted,
                            IsGovernmentProvided = docStatus?.DocumentType.IsGovernmentProvided ?? false,
                            GovernmentLink = docStatus?.DocumentType.GovernmentLink,
                            IsRequired = docStatus?.DocumentType.IsRequired ?? true,
                            SubmittedAt = docStatus?.SubmittedAt,
                            FilePath = docStatus?.FilePath
                        });
                    }
                }

                workflow.Steps.Add(stepDto);
            }
        }

        return Ok(workflow);
    }
}