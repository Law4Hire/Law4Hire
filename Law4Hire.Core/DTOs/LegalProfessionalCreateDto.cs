namespace Law4Hire.Core.DTOs;

public class LegalProfessionalCreateDto
{
    public required string Email { get; set; }
    public required string Password { get; set; } // Only used for seeding or admin creation
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string BarNumber { get; set; }
    public required string BarState { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
    public string? PreferredLanguage { get; set; }
}
