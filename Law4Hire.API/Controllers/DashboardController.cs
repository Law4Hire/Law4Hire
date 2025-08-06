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
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(Law4HireDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("visa-type/{userId:guid}")]
    public async Task<ActionResult<string>> GetUserSelectedVisaType(Guid userId)
    {
        // Get the user's completed interview state
        var interviewState = await _context.VisaInterviewStates
            .FirstOrDefaultAsync(vis => vis.UserId == userId && vis.IsCompleted);

        if (interviewState == null || string.IsNullOrEmpty(interviewState.SelectedVisaType))
        {
            return Ok(""); // Return empty string if no visa type selected
        }

        return Ok(interviewState.SelectedVisaType);
    }

    [HttpGet("documents/{userId:guid}")]
    public async Task<ActionResult<List<DocumentInfo>>> GetUserDocuments(Guid userId)
    {
        // Get the user's completed interview state
        var interviewState = await _context.VisaInterviewStates
            .FirstOrDefaultAsync(vis => vis.UserId == userId && vis.IsCompleted);

        if (interviewState == null || string.IsNullOrEmpty(interviewState.VisaWorkflowJson))
        {
            return Ok(new List<DocumentInfo>());
        }

        var documents = new List<DocumentInfo>();

        // Parse the workflow JSON to extract documents
        using var doc = JsonDocument.Parse(interviewState.VisaWorkflowJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("Workflow", out var workflowElement) && 
            workflowElement.TryGetProperty("Steps", out var steps))
        {
            foreach (var step in steps.EnumerateArray())
            {
                // Process government document
                if (step.TryGetProperty("GovernmentDocument", out var govDoc) && 
                    govDoc.ValueKind != JsonValueKind.Null)
                {
                    var documentName = govDoc.GetString();
                    if (!string.IsNullOrWhiteSpace(documentName))
                    {
                        documents.Add(new DocumentInfo
                        {
                            DocumentTypeId = Guid.NewGuid(),
                            DocumentName = documentName,
                            Status = "NotStarted", // Default status - will be updated by real status tracking
                            VisaType = interviewState.SelectedVisaType ?? "Unknown",
                            IsGovernmentProvided = true
                        });
                    }
                }

                // Process user-generated documents
                if (step.TryGetProperty("UserGeneratedDocuments", out var userDoc) && 
                    userDoc.ValueKind != JsonValueKind.Null)
                {
                    var documentName = userDoc.GetString();
                    if (!string.IsNullOrWhiteSpace(documentName))
                    {
                        documents.Add(new DocumentInfo
                        {
                            DocumentTypeId = Guid.NewGuid(),
                            DocumentName = documentName,
                            Status = "NotStarted", // Default status - will be updated by real status tracking
                            VisaType = interviewState.SelectedVisaType ?? "Unknown",
                            IsGovernmentProvided = false
                        });
                    }
                }
            }
        }

        return Ok(documents);
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
            VisaType = interviewState.SelectedVisaType ?? "Unknown"
        };

        // Get all document statuses for this user
        var documentStatuses = await _context.UserDocumentStatuses
            .Include(uds => uds.DocumentType)
            .Where(uds => uds.UserId == userId)
            .ToListAsync();

        // Parse workflow data according to prompt.txt format
        if (root.TryGetProperty("Workflow", out var workflowElement))
        {
            // Get totals from Workflow.Totals
            if (workflowElement.TryGetProperty("Totals", out var totals))
            {
                workflow.EstimatedTotalCost = totals.TryGetProperty("TotalEstimatedCost", out var totalCost)
                    ? totalCost.GetDecimal() : 0;
                workflow.EstimatedTotalTimeDays = totals.TryGetProperty("TotalEstimatedTime", out var totalTime)
                    ? (int)Math.Ceiling(totalTime.GetDecimal() / 1440) : 0; // Convert minutes to days
            }

            // Parse steps from Workflow.Steps
            if (workflowElement.TryGetProperty("Steps", out var steps))
            {
            foreach (var step in steps.EnumerateArray())
            {
                var stepDto = new WorkflowStepDto
                {
                    Name = step.TryGetProperty("StepName", out var name) ? (name.GetString() ?? "") : "",
                    Description = step.TryGetProperty("StepDescription", out var desc) ? (desc.GetString() ?? "") : "",
                    EstimatedCost = step.TryGetProperty("EstimatedCost", out var cost) ? cost.GetDecimal() : 0,
                    EstimatedTimeDays = step.TryGetProperty("EstimatedTime", out var time) 
                        ? (int)Math.Ceiling(time.GetDecimal() / 1440) : 0 // Convert minutes to days
                };

                // Add government link if this step has one
                if (step.TryGetProperty("GovernmentDocumentLink", out var govLink))
                {
                    stepDto.GovernmentLink = govLink.GetString();
                }

                // Add website link if this step has one
                if (step.TryGetProperty("WebsiteLink", out var webLink))
                {
                    stepDto.WebsiteLink = webLink.GetString();
                }

                // Process government document
                if (step.TryGetProperty("GovernmentDocument", out var govDoc) && 
                    govDoc.ValueKind != JsonValueKind.Null)
                {
                    var documentName = govDoc.GetString();
                    if (!string.IsNullOrWhiteSpace(documentName))
                    {
                        // Find corresponding document status
                        var docStatus = documentStatuses.FirstOrDefault(ds =>
                            ds.DocumentType.Name.Equals(documentName, StringComparison.OrdinalIgnoreCase));

                        stepDto.Documents.Add(new WorkflowDocumentDto
                        {
                            Id = Guid.NewGuid(),
                            Name = documentName,
                            DocumentName = documentName,
                            Status = docStatus?.Status ?? DocumentStatusEnum.NotStarted,
                            IsGovernmentProvided = true,
                            GovernmentLink = step.TryGetProperty("GovernmentDocumentLink", out var gdl) ? gdl.GetString() : null,
                            DownloadLink = step.TryGetProperty("GovernmentDocumentLink", out var gdl2) ? gdl2.GetString() : null,
                            IsRequired = true,
                            SubmittedAt = docStatus?.SubmittedAt,
                            FilePath = docStatus?.FilePath
                        });
                    }
                }

                // Process user-generated documents
                if (step.TryGetProperty("UserGeneratedDocuments", out var userDoc) && 
                    userDoc.ValueKind != JsonValueKind.Null)
                {
                    var documentName = userDoc.GetString();
                    if (!string.IsNullOrWhiteSpace(documentName))
                    {
                        // Find corresponding document status
                        var docStatus = documentStatuses.FirstOrDefault(ds =>
                            ds.DocumentType.Name.Equals(documentName, StringComparison.OrdinalIgnoreCase));

                        stepDto.Documents.Add(new WorkflowDocumentDto
                        {
                            Id = Guid.NewGuid(),
                            Name = documentName,
                            DocumentName = documentName,
                            Status = docStatus?.Status ?? DocumentStatusEnum.NotStarted,
                            IsGovernmentProvided = false,
                            GovernmentLink = null,
                            DownloadLink = null,
                            IsRequired = true,
                            SubmittedAt = docStatus?.SubmittedAt,
                            FilePath = docStatus?.FilePath
                        });
                    }
                }

                workflow.Steps.Add(stepDto);
            }
        }
        }

        return Ok(workflow);
    }

    [HttpGet("workflow-steps/{userId:guid}")]
    public async Task<ActionResult<List<WorkflowStepDto>>> GetUserWorkflowSteps(Guid userId)
    {
        try
        {
            // Check if user has completed visa interview
            var interviewState = await _context.VisaInterviewStates
                .FirstOrDefaultAsync(vis => vis.UserId == userId && vis.IsCompleted);

            if (interviewState == null || string.IsNullOrEmpty(interviewState.SelectedVisaType))
            {
                return Ok(new List<WorkflowStepDto>()); // Return empty list if no completed interview
            }

            // Get workflow steps from database
            var steps = await _context.WorkflowSteps
                .Include(ws => ws.Documents)
                .Where(ws => ws.UserId == userId && ws.VisaType == interviewState.SelectedVisaType)
                .OrderBy(ws => ws.StepNumber)
                .ToListAsync();

            var result = steps.Select(step => new WorkflowStepDto
            {
                Id = step.Id,
                StepNumber = step.StepNumber,
                Name = step.Name,
                Description = step.Description,
                EstimatedCost = step.EstimatedCost,
                EstimatedTimeDays = step.EstimatedTimeDays,
                WebsiteLink = step.WebsiteLink,
                Status = step.Status,
                Documents = step.Documents.Select(doc => new WorkflowDocumentDto // Changed from WorkflowStepDocumentDto
                {
                    Id = doc.Id,
                    Name = doc.DocumentName,
                    DocumentName = doc.DocumentName,
                    IsGovernmentProvided = doc.IsGovernmentProvided,
                    DownloadLink = doc.DownloadLink,
                    IsRequired = doc.IsRequired,
                    Status = doc.Status,
                    FilePath = doc.FilePath,
                    SubmittedAt = doc.SubmittedAt,
                }).ToList()
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error loading workflow steps: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    private string GetStatusColor(DocumentStatusEnum status)
    {
        return status switch
        {
            DocumentStatusEnum.NotStarted => "text-red-500",
            DocumentStatusEnum.InProgress => "text-black",
            DocumentStatusEnum.Submitted => "text-black",
            DocumentStatusEnum.Approved => "text-green-500",
            DocumentStatusEnum.Rejected => "text-red-500",
            _ => "text-black"
        };
    }

    private string GetStatusText(DocumentStatusEnum status)
    {
        return status switch
        {
            DocumentStatusEnum.NotStarted => "Not Started",
            DocumentStatusEnum.InProgress => "In Progress",
            DocumentStatusEnum.Submitted => "Submitted",
            DocumentStatusEnum.Approved => "Approved",
            DocumentStatusEnum.Rejected => "Rejected",
            _ => "Unknown"
        };
    }
}