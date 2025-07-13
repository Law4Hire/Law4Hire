using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class VisaTypeQuestion
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid VisaTypeId { get; set; }
    public VisaType VisaType { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string QuestionKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public int Order { get; set; }

    public bool IsRequired { get; set; }

    [MaxLength(1000)]
    public string? ValidationRules { get; set; }
}
