﻿using System;
using Microsoft.AspNetCore.Identity;

namespace Law4Hire.Core.Entities;

public class User : IdentityUser<Guid> // Inherits from IdentityUser with Guid as the key type
{
    public new Guid Id { get; set; }
    public new string Email { get; set; } = default!;
    public new string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public new string? PhoneNumber { get; set; }
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
    public new byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public string? Category { get; set; }
    public string? VisaType { get; set; }
    public string? WorkflowJson { get; set; }

    // Navigation properties
    public ICollection<IntakeSession> IntakeSessions { get; set; } = new List<IntakeSession>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public ICollection<UserDocumentStatus> Documents { get; set; } = new List<UserDocumentStatus>();
    public VisaInterviewState? VisaInterview { get; set; }
}