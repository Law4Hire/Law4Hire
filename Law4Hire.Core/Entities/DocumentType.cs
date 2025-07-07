namespace Law4Hire.Core.Entities;

public class DocumentType
{
    public Guid Id { get; set; }
    public string FormNumber { get; set; } = null!; // e.g., I-129, DS-160
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IssuingAgency { get; set; } = null!; // e.g., USCIS, DOS, CBP

    //public ICollection<VisaDocumentRequirement> RequiredForVisas { get; set; } = new List<VisaDocumentRequirement>();
    //public ICollection<UserDocumentStatus> UserStatuses { get; set; } = new List<UserDocumentStatus>();
    //public ICollection<UserDocumentStatus> DocumentStatuses { get; set; } = new List<UserDocumentStatus>();
    //public ICollection<VisaDocumentRequirement> VisaRequirements { get; set; } = new List<VisaDocumentRequirement>();
    public ICollection<VisaDocumentRequirement> VisaDocumentRequirements { get; set; } = new List<VisaDocumentRequirement>();
    public ICollection<UserDocumentStatus> UserDocumentStatuses { get; set; } = new List<UserDocumentStatus>();

}
