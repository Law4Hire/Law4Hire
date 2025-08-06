using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class VisaWizard
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Purpose { get; set; } = string.Empty;
    
    // Answer values for follow-up questions (only populated if there are follow-ups)
    public string? Answer1 { get; set; }
    public string? Answer2 { get; set; }
    
    // Indicates if this answer leads to a follow-up question  
    public bool HasFollowUp { get; set; }
    
    // Learn More links extracted from the page
    public string? LearnMoreLinks { get; set; }
    
    // Final outcome content when wizard completes
    public string? OutcomeDisplayContent { get; set; }
    
    // Visa recommendations from the outcome
    public string? VisaRecommendations { get; set; }
    
    // Related visa categories
    public string? RelatedVisaCategories { get; set; }
    
    // Session tracking
    public Guid SessionId { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int StepNumber { get; set; }
    
    public bool IsCompleteSession { get; set; }
}