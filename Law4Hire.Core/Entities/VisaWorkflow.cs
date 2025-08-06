namespace Law4Hire.Core.Entities;

/// <summary>
/// Represents a scraped visa workflow for a specific visa type and country.
/// </summary>
public class VisaWorkflow
{
    public Guid Id { get; set; }

    public Guid CountryId { get; set; }

    public Guid VisaTypeId { get; set; }

    public string WorkflowJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Hash { get; set; }
}