// Add these entities to your Core/Entities folder

using Law4Hire.Core.Entities;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string VisaType { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public int EstimatedTimeDays { get; set; }
    public string? WebsiteLink { get; set; }
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WorkflowStepDocument> Documents { get; set; } = new List<WorkflowStepDocument>();
}
public class WorkflowResult
{
    public List<WorkflowStep> Steps { get; set; } = new();
    public decimal EstimatedTotalCost { get; set; }
    public int EstimatedTotalTimeDays { get; set; }
}

public class WorkflowStepDocument
{
    public Guid Id { get; set; }
    public Guid WorkflowStepId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public bool IsGovernmentProvided { get; set; }
    public string? DownloadLink { get; set; }
    public bool IsRequired { get; set; } = true;
    public DocumentStatusEnum Status { get; set; } = DocumentStatusEnum.NotStarted;
    public string? FilePath { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public WorkflowStep WorkflowStep { get; set; } = null!;
}

public enum WorkflowStepStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}

public enum DocumentStatusEnum
{
    NotStarted = 0,
    InProgress = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4
}



