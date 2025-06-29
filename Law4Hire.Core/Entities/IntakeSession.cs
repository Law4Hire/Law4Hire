using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class IntakeSession
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!; // Required navigation property

    public IntakeStatus Status { get; set; } = IntakeStatus.Started;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(10)]
    public string Language { get; set; } = "en-US";

    [MaxLength(4000)] // Store session data as JSON string, e.g., progress, temporary answers
    public string? SessionData { get; set; }

    // Navigation property for responses
    public ICollection<IntakeResponse> Responses { get; set; } = new List<IntakeResponse>();
}

public class IntakeResponse
{
    public int Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }
    public IntakeSession Session { get; set; } = null!;

    [Required]
    public int QuestionId { get; set; }
    public IntakeQuestion Question { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string ResponseText { get; set; } = string.Empty;

    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}


