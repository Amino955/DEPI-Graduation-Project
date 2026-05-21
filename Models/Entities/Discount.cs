using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class Discount : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DiscountTarget Target { get; set; }

    // Only set if Target = Product
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Only set if Target = Category
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DiscountType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int UsageCount { get; set; } = 0;

    [NotMapped]
    public bool IsCurrentlyActive => IsActive
        && !IsDeleted
        && DateTime.UtcNow >= StartDate
        && DateTime.UtcNow <= EndDate;
}
