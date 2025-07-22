namespace Law4Hire.Core.Entities;

public class DocumentType
{
    public Guid Id { get; set; }
    public string FormNumber { get; set; } = null!; 
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IssuingAgency { get; set; } = null!;
    public bool IsGovernmentProvided  { get; set; }
    public string? GovernmentLink { get; set; } 
    public bool IsRequired { get; set; } = true;

    //public ICollection<VisaDocumentRequirement> RequiredForVisas { get; set; } = new List<VisaDocumentRequirement>();
    //public ICollection<UserDocumentStatus> UserStatuses { get; set; } = new List<UserDocumentStatus>();
    //public ICollection<UserDocumentStatus> DocumentStatuses { get; set; } = new List<UserDocumentStatus>();
    //public ICollection<VisaDocumentRequirement> VisaRequirements { get; set; } = new List<VisaDocumentRequirement>();
    public ICollection<VisaDocumentRequirement> VisaDocumentRequirements { get; set; } = new List<VisaDocumentRequirement>();
    public ICollection<UserDocumentStatus> UserDocumentStatuses { get; set; } = new List<UserDocumentStatus>();

}
