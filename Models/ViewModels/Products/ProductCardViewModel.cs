namespace TellaStore.Models.ViewModels.Products;

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool HasDiscount => FinalPrice < BasePrice;
    public string? MainImageUrl { get; set; }
    public bool IsInStock { get; set; }
    public bool IsFeatured { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsInWishlist { get; set; }
}
