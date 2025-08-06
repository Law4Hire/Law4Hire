using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class UserServicePackage
{
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int ServicePackageId { get; set; }
    public ServicePackage ServicePackage { get; set; } = null!;

    public bool Paid { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }
}