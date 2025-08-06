namespace Law4Hire.Core.DTOs;

public record UserProfileDto(
    Guid Id,
    Guid UserId,
    DateTime? DateOfBirth,
    Guid? CitizenshipCountryId,
    string? CitizenshipCountryName,
    string? MaritalStatus,
    bool? HasRelativesInUS,
    bool? HasJobOffer,
    string? EducationLevel,
    bool? FearOfPersecution,
    bool? HasPastVisaDenials,
    bool? HasStatusViolations,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateUserProfileDto(
    Guid UserId,
    DateTime? DateOfBirth,
    Guid? CitizenshipCountryId,
    string? MaritalStatus,
    bool? HasRelativesInUS,
    bool? HasJobOffer,
    string? EducationLevel,
    bool? FearOfPersecution,
    bool? HasPastVisaDenials,
    bool? HasStatusViolations,
    string? Notes
);

public record UpdateUserProfileDto
{
    public DateTime? DateOfBirth { get; set; }
    public Guid? CitizenshipCountryId { get; set; }
    public string? MaritalStatus { get; set; }
    public bool? HasRelativesInUS { get; set; }
    public bool? HasJobOffer { get; set; }
    public string? EducationLevel { get; set; }  
    public bool? FearOfPersecution { get; set; }
    public bool? HasPastVisaDenials { get; set; }
    public bool? HasStatusViolations { get; set; }
    public string? Notes { get; set; }
}