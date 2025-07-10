using Law4Hire.Core.Entities;

public class LegalProfessional
{
    public Guid Id { get; set; } // FK to User.Id
    public string BarNumber { get; set; } = string.Empty; // Optional
    public string BarState { get; set; } = string.Empty; // Licensing state

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}