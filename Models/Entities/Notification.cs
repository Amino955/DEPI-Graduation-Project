using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class Notification : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public string? Link { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
