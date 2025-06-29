using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class IntakeQuestion
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string QuestionKey { get; set; } = string.Empty; // e.g., "full_name", "date_of_birth"

    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public int Order { get; set; }

    public int? ParentQuestionId { get; set; } // For conditional questions
    public IntakeQuestion? ParentQuestion { get; set; }

    public bool IsRequired { get; set; }

    [MaxLength(1000)]
    public string? ValidationRules { get; set; } // JSON string for client-side validation rules

    [MaxLength(1000)]
    public string? Conditions { get; set; } // JSON string for conditional display logic (e.g., {"showIf": {"question_key": ["value"]}})

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    public ICollection<IntakeResponse> Responses { get; set; } = new List<IntakeResponse>();
}
