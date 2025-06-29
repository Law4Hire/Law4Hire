using System.ComponentModel.DataAnnotations;
using Law4Hire.Core.Enums;

namespace Law4Hire.Core.Entities;

public class ServicePackage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public PackageType Type { get; set; }

    [Range(0.01, (double)decimal.MaxValue)]
    public decimal BasePrice { get; set; }

    public bool HasMoneyBackGuarantee { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastModifiedAt { get; set; }
}