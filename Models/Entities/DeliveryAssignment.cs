using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class DeliveryAssignment : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public string DeliveryUserId { get; set; } = string.Empty;
    public ApplicationUser DeliveryUser { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }

    public DeliveryAssignmentStatus Status { get; set; } = DeliveryAssignmentStatus.Assigned;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public string? ProofImageUrl { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }
}
