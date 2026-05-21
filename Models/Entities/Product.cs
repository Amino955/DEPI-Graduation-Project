using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class Product : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    public Season Season { get; set; } = Season.AllSeason;
    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();

    [NotMapped]
    public double AverageRating => Reviews.Any(r => r.IsApproved) ? Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0;

    [NotMapped]
    public int TotalStock => Variants.Where(v => !v.IsDeleted && v.IsActive).Sum(v => v.Stock);

    [NotMapped]
    public bool IsInStock => TotalStock > 0;
}
