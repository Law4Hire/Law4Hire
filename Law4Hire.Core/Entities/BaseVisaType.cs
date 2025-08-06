using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Base visa types with comprehensive information
/// Restructured to match the Updated.json format
/// </summary>
public class BaseVisaType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Visa code (e.g., "H-1B", "F-1", "EB-5")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive name of the visa type
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the visa type
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// First qualifying question
    /// </summary>
    public string? Question1 { get; set; }

    /// <summary>
    /// Second qualifying question
    /// </summary>
    public string? Question2 { get; set; }

    /// <summary>
    /// Third qualifying question
    /// </summary>
    public string? Question3 { get; set; }

    /// <summary>
    /// Indicates if this visa type is still current/valid
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Deprecated

    /// <summary>
    /// When this visa type was first created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this visa type was updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Confidence score from Bruce (0.0 to 1.0)
    /// </summary>
    public decimal ConfidenceScore { get; set; } = 1.0m;

    // Navigation properties
    public List<ServicePackage> ServicePackages { get; set; } = new();
    public List<CategoryVisaType> CategoryVisaTypes { get; set; } = new();
}