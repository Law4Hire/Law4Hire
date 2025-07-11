using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class ScrapeLog
{
    [Key]
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityAffected { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
