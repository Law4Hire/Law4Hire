using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

/// <summary>
/// Extended user profile information for visa eligibility assessment
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// User's date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's citizenship country
    /// </summary>
    public Guid? CitizenshipCountryId { get; set; }
    public Country? CitizenshipCountry { get; set; }

    /// <summary>
    /// User's marital status
    /// </summary>
    [MaxLength(20)]
    public string? MaritalStatus { get; set; } // Single, Married, Divorced, Widowed

    /// <summary>
    /// Does the user have relatives living in the US
    /// </summary>
    public bool? HasRelativesInUS { get; set; }

    /// <summary>
    /// Does the user have a job offer in the US
    /// </summary>
    public bool? HasJobOffer { get; set; }

    /// <summary>
    /// User's education level
    /// </summary>
    [MaxLength(50)]
    public string? EducationLevel { get; set; } // Less than High School, High School, Some College, Bachelor's, Master's, Doctorate, Professional

    /// <summary>
    /// Does the user fear persecution in their home country
    /// </summary>
    public bool? FearOfPersecution { get; set; }

    /// <summary>
    /// Has the user had past visa denials
    /// </summary>
    public bool? HasPastVisaDenials { get; set; }

    /// <summary>
    /// Has the user had status violations
    /// </summary>
    public bool? HasStatusViolations { get; set; }

    /// <summary>
    /// Additional notes about the user's situation
    /// </summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}