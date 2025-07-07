using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class UserDocumentStatus
{
    public Guid Id { get; set; }

    public Guid UserVisaId { get; set; }
    public UserVisa UserVisa { get; set; } = null!;

    public Guid DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = null!;

    public DocumentStatusEnum Status { get; set; } = DocumentStatusEnum.NotStarted;
    public DateTime? SubmittedAt { get; set; }
    public string? FilePath { get; set; } // For linking to uploaded file in storage
    public Guid UserId { get; set; }
    public Guid VisaTypeId { get; set; }
    public VisaType VisaType { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

}


