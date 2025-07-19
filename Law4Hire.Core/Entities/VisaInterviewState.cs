namespace Law4Hire.Core.Entities;

public class VisaInterviewState
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? CurrentVisaOptionsJson { get; set; }
    public string? SelectedVisaType { get; set; }
    public string? VisaWorkflowJson { get; set; }
    public string? ExtractedDocumentsJson { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? LastClientMessage { get; set; }
    public string? LastBotMessage { get; set; }
}
