// Add these DTOs to your Core/DTOs folder

using Law4Hire.Core.Enums;

public class DashboardWorkflowDto
{
    public string VisaType { get; set; } = string.Empty;
    public List<WorkflowStepDto> Steps { get; set; } = new();
    public decimal EstimatedTotalCost { get; set; }
    public int EstimatedTotalTimeDays { get; set; }
}

public class WorkflowStepDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowDocumentDto> Documents { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    public int EstimatedTimeDays { get; set; }
    public string? GovernmentLink { get; set; }
}

public class WorkflowDocumentDto
{
    public string Name { get; set; } = string.Empty;
    public DocumentStatusEnum Status { get; set; }
    public bool IsGovernmentProvided { get; set; }
    public string? GovernmentLink { get; set; }
    public bool IsRequired { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? FilePath { get; set; }

    public string StatusColor => Status switch
    {
        DocumentStatusEnum.NotStarted => "text-red-500",
        DocumentStatusEnum.InProgress => "text-black",
        DocumentStatusEnum.Submitted => "text-black",
        DocumentStatusEnum.Approved => "text-green-500",
        DocumentStatusEnum.Rejected => "text-red-500",
        _ => "text-black"
    };

    public string StatusText => Status switch
    {
        DocumentStatusEnum.NotStarted => "Not Started",
        DocumentStatusEnum.InProgress => "In Progress",
        DocumentStatusEnum.Submitted => "Submitted",
        DocumentStatusEnum.Approved => "Approved",
        DocumentStatusEnum.Rejected => "Rejected",
        _ => "Unknown"
    };
}