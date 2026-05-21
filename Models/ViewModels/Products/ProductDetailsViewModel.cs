using TellaStore.Models.Entities;

namespace TellaStore.Models.ViewModels.Products;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public List<ProductVariant> Variants { get; set; } = new();
    public List<string> AvailableColors { get; set; } = new();
    public decimal FinalPrice { get; set; }
    public bool HasDiscount { get; set; }
    public decimal OriginalPrice { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public bool IsInWishlist { get; set; }
    public bool CanReview { get; set; }
    public List<ProductCardViewModel> RelatedProducts { get; set; } = new();
}
