using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Base visa types discovered by the Bruce scraper agent
/// Contains the raw visa type names organized by category and sub-categories
/// </summary>
public class BaseVisaType
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "H-1B", "F-1", "EB-5"

    [Required]
    public Guid CategoryId { get; set; }
    public VisaCategory Category { get; set; } = null!;

    /// <summary>
    /// JSON array of sub-category names that led to this visa type being discovered
    /// </summary>
    [MaxLength(2000)]
    public string? RelatedSubCategories { get; set; }

    /// <summary>
    /// Indicates if this visa type is still current/valid
    /// </summary>
    public string Status { get; set; } = "Active"; // Active, Deprecated, Removed

    /// <summary>
    /// When this visa type was first discovered by Bruce
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time Bruce confirmed this visa type still exists
    /// </summary>
    public DateTime LastConfirmedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Confidence score from Bruce (0.0 to 1.0)
    /// </summary>
    public decimal ConfidenceScore { get; set; } = 1.0m;
}