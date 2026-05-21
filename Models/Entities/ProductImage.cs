using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty;    // Cloudinary HTTPS URL

    public string? PublicId { get; set; }                   // Cloudinary PublicId (needed for deletion)

    public bool IsMain { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
