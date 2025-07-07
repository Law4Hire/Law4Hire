using System;
using Microsoft.AspNetCore.Identity;

namespace Law4Hire.Core.Entities;

public class User : IdentityUser<Guid> // Inherits from IdentityUser with Guid as the key type
{
    // Explicitly override common IdentityUser properties so we can control
    // default values while avoiding compiler warnings about hidden members.
    public override Guid Id { get; set; }
    public override string? Email { get; set; } = default!;
    public override string? UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public override string? PhoneNumber { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // ✅ Newly added address and identity fields
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    // Use our own password storage mechanism rather than Identity's string
    // representation. Hide the base property with the 'new' keyword.
    public new byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public ICollection<IntakeSession> IntakeSessions { get; set; } = new List<IntakeSession>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
