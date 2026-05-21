using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    [MaxLength(7)]
    public string ColorHex { get; set; } = "#000000";

    [MaxLength(20)]
    public string Size { get; set; } = string.Empty;

    public SizeType SizeType { get; set; } = SizeType.Letters;

    public int Stock { get; set; } = 0;

    // Final price = Product.BasePrice + PriceModifier
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceModifier { get; set; } = 0;

    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    // NOTE: Cart is stored in Session — there is NO CartItem entity in the database
}
