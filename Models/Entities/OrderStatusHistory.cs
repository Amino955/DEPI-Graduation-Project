using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public string? ChangedByUserId { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }
}
