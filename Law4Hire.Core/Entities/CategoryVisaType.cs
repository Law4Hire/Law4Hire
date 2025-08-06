using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Cross-reference table linking visa types to categories
/// Allows a single visa type to belong to multiple categories
/// </summary>
public class CategoryVisaType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CategoryId { get; set; }
    public VisaCategory Category { get; set; } = null!;

    [Required]
    public Guid VisaTypeId { get; set; }
    public BaseVisaType VisaType { get; set; } = null!;

    /// <summary>
    /// When this association was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this association is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}