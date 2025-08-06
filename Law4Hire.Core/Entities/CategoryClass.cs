using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Represents visa classes (the letter part before the dash in visa codes)
/// For example: H (from H-1B), F (from F-1), etc.
/// Used for initial filtering and narrowing of visa options
/// </summary>
public class CategoryClass
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The class letter (e.g., "H", "F", "B", "TN")
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ClassCode { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive name of the class
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// General description of what this class is for
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// General category (e.g., "Work", "Study", "Visit", "Immigration")
    /// </summary>
    [MaxLength(50)]
    public string? GeneralCategory { get; set; }

    /// <summary>
    /// When this class was first created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this class was updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this class is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public List<BaseVisaType> VisaTypes { get; set; } = new();
}