using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class VisaGroup
{
    [Key]
    public Guid Id { get; set; }
    public VisaGroupEnum Name { get; set; }
    public string? Description { get; set; }

    public ICollection<VisaType> VisaTypes { get; set; } = new List<VisaType>();
}
