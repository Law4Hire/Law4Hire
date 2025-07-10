using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class VisaType
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!; // legacy category field

    public Guid VisaGroupId { get; set; }
    public VisaGroup VisaGroup { get; set; } = null!;

    public ICollection<VisaDocumentRequirement> DocumentRequirements { get; set; } = new List<VisaDocumentRequirement>();
    public ICollection<UserDocumentStatus> UserDocumentStatuses { get; set; } = new List<UserDocumentStatus>();
    public ICollection<UserVisa> UserVisas { get; set; } = new List<UserVisa>();
   // public ICollection<VisaDocumentRequirement> RequiredDocuments { get; set; } = new List<VisaDocumentRequirement>();

}
