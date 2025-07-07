using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class VisaDocumentRequirement
{
    [Key]
    public Guid Id { get; set; }

    public Guid VisaTypeId { get; set; }
    public VisaType VisaType { get; set; } = null!;

    public Guid DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = null!;

    public bool IsRequired { get; set; }
}
