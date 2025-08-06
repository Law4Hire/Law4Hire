using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Lookup table for US states and territories
/// </summary>
public class USState
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string StateCode { get; set; } = string.Empty; // e.g., "CA", "NY", "TX"

    /// <summary>
    /// Indicates if this is a state (true) or territory (false)
    /// </summary>
    public bool IsState { get; set; } = true;

    /// <summary>
    /// Indicates if this state/territory is active in the system
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order for dropdown display
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}