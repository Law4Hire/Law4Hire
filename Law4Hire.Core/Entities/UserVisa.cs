namespace Law4Hire.Core.Entities;

public class UserVisa
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid VisaTypeId { get; set; }
    public VisaType VisaType { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public ICollection<UserDocumentStatus> DocumentStatuses { get; set; } = new List<UserDocumentStatus>();
}
