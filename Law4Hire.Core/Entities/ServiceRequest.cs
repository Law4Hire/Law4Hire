using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class ServiceRequest
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int ServicePackageId { get; set; }
    public ServicePackage ServicePackage { get; set; } = null!;

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.PendingPayment;

    public decimal AgreedPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // JSON string storing details about the request, e.g., intake session ID, specific client needs
    [MaxLength(4000)]
    public string? RequestDetails { get; set; }

    // Navigation property for forms required/generated for this request
    public ICollection<RequiredForm> RequiredForms { get; set; } = new List<RequiredForm>();
}

public class RequiredForm
{
    public int Id { get; set; }

    [Required]
    public Guid ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string FormName { get; set; } = string.Empty; // e.g., "I-130", "I-485"

    public FormStatus Status { get; set; } = FormStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public enum ServiceRequestStatus
{
    PendingPayment = 1,
    PaymentReceived = 2,
    InProgress = 3,
    AwaitingClientInfo = 4,
    AwaitingReview = 5,
    Completed = 6,
    Cancelled = 7,
    OnHold = 8
}

public enum FormStatus
{
    Pending = 1,
    InProgress = 2,
    AwaitingClientInput = 3,
    ReadyForReview = 4,
    Submitted = 5,
    Approved = 6,
    Rejected = 7,
    Archived = 8
}
