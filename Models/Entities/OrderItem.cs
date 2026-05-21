using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;

    // SNAPSHOT at order time — price/name changes later will NOT affect this order
    [Required, MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Size { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [NotMapped]
    public decimal TotalPrice => UnitPrice * Quantity;
}
