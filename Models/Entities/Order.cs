using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class Order : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // IMPORTANT: Shipping address is COPIED at order time
    // This ensures past orders are not affected by future address changes
    [Required, MaxLength(200)]
    public string ShippingStreet { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ShippingGovernorate { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShippingNotes { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // 4-digit code shown to customer — delivery person must enter this to confirm delivery
    [MaxLength(4)]
    public string? DeliveryCode { get; set; }
    public bool IsCodeVerified { get; set; } = false;

    [MaxLength(500)]
    public string? AdminNotes { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public DeliveryAssignment? DeliveryAssignment { get; set; }
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
