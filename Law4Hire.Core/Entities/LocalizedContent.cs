using System.ComponentModel.DataAnnotations;

namespace Law4Hire.Core.Entities;

public class LocalizedContent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ContentKey { get; set; } = string.Empty; // e.g., "welcome_message", "privacy_notice"

    [Required]
    [MaxLength(10)]
    public string Language { get; set; } = string.Empty; // e.g., "en-US", "es-ES"

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
