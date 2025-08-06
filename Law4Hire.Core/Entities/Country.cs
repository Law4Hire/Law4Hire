using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Lookup table for UN-recognized countries
/// </summary>
public class Country
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(3)]
    public string CountryCode { get; set; } = string.Empty; // ISO 3166-1 alpha-3

    [Required]
    [MaxLength(2)]
    public string CountryCode2 { get; set; } = string.Empty; // ISO 3166-1 alpha-2

    /// <summary>
    /// Indicates if this country is recognized by the UN
    /// </summary>
    public bool IsUNRecognized { get; set; } = true;

    /// <summary>
    /// Indicates if this country is active in the system
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order for dropdown display
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}