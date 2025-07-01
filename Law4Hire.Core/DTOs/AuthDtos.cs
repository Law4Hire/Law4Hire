using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.DTOs;

public class UserRegistrationDto
{
    // Existing properties...
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Add these new properties:
    public string? MiddleName { get; set; }
    public string? Address { get; set; }
    public string? ImmigrationGoal { get; set; }
}

public class UserLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}