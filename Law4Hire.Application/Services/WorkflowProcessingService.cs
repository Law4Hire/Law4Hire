using Law4Hire.Core.Entities;
using Law4Hire.Core.Enums;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Law4Hire.Application.Services
{
    public class WorkflowProcessingService
    {
        private readonly Law4HireDbContext _context;

        public WorkflowProcessingService(Law4HireDbContext context)
        {
            _context = context;
        }

        public async Task ProcessWorkflowSteps(Guid userId, string visaType, string workflowJson)
        {
            // Remove existing workflow steps for this user/visa type
            var existingSteps = await _context.WorkflowSteps
                .Include(ws => ws.Documents)
                .Where(ws => ws.UserId == userId && ws.VisaType == visaType)
                .ToListAsync();

            if (existingSteps.Any())
            {
                _context.WorkflowSteps.RemoveRange(existingSteps);
                await _context.SaveChangesAsync();
            }

            // Parse the workflow JSON with new Steps/Totals schema
            using var doc = JsonDocument.Parse(workflowJson);
            var root = doc.RootElement;
            var steps = root.GetProperty("Steps");

            int stepNumber = 1;
            foreach (var step in steps.EnumerateArray())
            {
                var workflowStep = new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    VisaType = visaType,
                    StepNumber = stepNumber++,
                    Name = step.GetProperty("name").GetString() ?? "Unknown Step",
                    Description = step.GetProperty("description").GetString() ?? "",
                    EstimatedCost = step.TryGetProperty("estimatedCost", out var cost) ? cost.GetDecimal() : 0,
                    EstimatedTimeDays = step.TryGetProperty("estimatedTimeDays", out var time) ? time.GetInt32() : 0,
                    WebsiteLink = step.TryGetProperty("websiteLink", out var link) ? link.GetString() : null,
                    Status = WorkflowStepStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WorkflowSteps.Add(workflowStep);

                // Process documents for this step
                if (step.TryGetProperty("documents", out var documents) && documents.ValueKind == JsonValueKind.Array)
                {
                    foreach (var document in documents.EnumerateArray())
                    {
                        var stepDocument = new WorkflowStepDocument
                        {
                            Id = Guid.NewGuid(),
                            WorkflowStepId = workflowStep.Id,
                            DocumentName = document.GetProperty("name").GetString() ?? "Unknown Document",
                            IsGovernmentProvided = document.TryGetProperty("isGovernmentProvided", out var isGov) && isGov.GetBoolean(),
                            DownloadLink = document.TryGetProperty("downloadLink", out var dlLink) ? dlLink.GetString() : null,
                            IsRequired = !document.TryGetProperty("isRequired", out var isReq) || isReq.GetBoolean(),
                            Status = DocumentStatusEnum.NotStarted,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.WorkflowStepDocuments.Add(stepDocument);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // Add the missing ProcessWorkflowDocuments method
        public async Task ProcessWorkflowDocuments(Guid userId, string visaType, string workflowJson)
        {
            // This method can be an alias for ProcessWorkflowSteps or handle documents specifically
            await ProcessWorkflowSteps(userId, visaType, workflowJson);
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

   
}