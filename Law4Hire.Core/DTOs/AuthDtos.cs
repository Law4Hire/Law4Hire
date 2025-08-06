using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.DTOs;

public class UserRegistrationDto
{
    // Basic information
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    
    // Address information
    public string Address1 { get; set; } = string.Empty;
    public string Address2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    
    // Personal information
    public DateTime DateOfBirth { get; set; }
    public Guid? CitizenshipCountryId { get; set; }
    public string? MaritalStatus { get; set; }
    
    // Extended profile information
    public bool? HasRelativesInUS { get; set; }
    public bool? HasJobOffer { get; set; }
    public string? EducationLevel { get; set; }
    public bool? FearOfPersecution { get; set; }
    public bool? HasPastVisaDenials { get; set; }
    public bool? HasStatusViolations { get; set; }
    
    public string? ImmigrationGoal { get; set; } = string.Empty;
}

public class UserLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}